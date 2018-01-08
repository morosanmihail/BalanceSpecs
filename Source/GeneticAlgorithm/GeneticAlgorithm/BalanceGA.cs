using GeneticAlgorithm.Evaluators;
using GeneticAlgorithm.RemoteControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpGenetics.BaseClasses;
using SharpGenetics.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Accord.Math;

namespace GeneticAlgorithm.GeneticAlgorithm
{
    [DataContract]
    public class BalanceGA : PopulationMember
    {
        PopulationManager<BalanceGA> Manager;

        [DataMember]
        public String CreatedBy = "";

        [DataMember]
        public string Results = "";

        public BalanceGA(PopulationManager<BalanceGA> Manager, List<double> root = null, CRandom rand = null)
        {
            UpdatedAtGeneration = -1;

            if (rand != null)
            {
                this.rand = new CRandom(rand.Next());
            }

            ReloadParameters(Manager);

            Vector = root;

            //Generate random one
            if (root == null)
            {
                Vector = GenerateData();
                CreatedBy = "Random";
            }

            this.CreatedAtGeneration = Manager.GenerationsRun + 1;
        }

        public override void ReloadParameters<T>(PopulationManager<T> Manager)
        {
            this.Manager = Manager as PopulationManager<BalanceGA>;
        }

        List<double> GenerateData()
        {
            List<double> data = new List<double>();

            dynamic Params = Manager.GetParameters().JsonParams.parameters;

            foreach (var P in Params)
            {
                if ((bool)P.enabled == true)
                {
                    int ListSize = P.listsize != null ? (int)P.listsize : 1;

                    if (P.distinct == null || (bool)P.distinct == false)
                    {
                        for (int i = 0; i < ListSize; i++)
                        {
                            data.Add(GetValInRange((double)P.rangeMin, (double)P.rangeMax, (int)P.rangeAccuracy));
                        }
                    }
                    else
                    {
                        //Only works with 1 parameter
                        var Allowed = new List<double>();
                        for (int i = (int)P.rangeMin; i <= (int)P.rangeMax; i++)
                        {
                            Allowed.Add(i);
                        }

                        for (int i = 0; i < ListSize; i++)
                        {
                            data.Add(Allowed[rand.Next(0, Allowed.Count)]);
                            Allowed.Remove(data.Last());
                        }
                    }
                }
            }

            return data;
        }

        double GetValInRange(double ValueRangeMin, double ValueRangeMax, int ValueRangeAccuracy)
        {
            return Math.Round(rand.Next((int)(ValueRangeMin * ValueRangeAccuracy), (int)(ValueRangeMax * ValueRangeAccuracy)) / (float)ValueRangeAccuracy, (int)Math.Log10(ValueRangeAccuracy));
        }

        public override double CalculateFitness(int CurrentGeneration)
        {
            int RandSeed = rand.Next();
            if ((this.Fitness < 0 || (this.Predicted && this.RealFitness < 0)) || Manager.RecalculateAfterAGeneration)
            {
                if (this.UpdatedAtGeneration < CurrentGeneration)
                {
                    this.Evaluations++;

                    var CalculatedFitnessScores = RunGames(CurrentGeneration, RandSeed);
                    if (!Predicted)
                    {
                        ObjectivesFitness = CalculatedFitnessScores;
                    } else
                    {
                        RealFitness = CalculatedFitnessScores.Sum();
                    }

                    this.UpdatedAtGeneration = CurrentGeneration;
                }
            }

            return this.Fitness;
        }

        List<double> RunGames(int CurrentGeneration, int RandSeed)
        {
            string NewResults = "";
            if ((string)Manager.GetParameters().JsonParams.bridge.type == "local")
            {
                NewResults = RunGamesLocal(Manager.GetParameters().JsonParams, Vector, RandSeed);
            }
            else
            {
                NewResults = RunGamesRemote(Manager.GetParameters().JsonParams, Vector, RandSeed);
            }

            var JsonParams = Manager.GetParameters().JsonParams;

            CombineResults(NewResults, JsonParams);

            return EvaluateResults(JsonParams, Results, Vector);
        }

        void CombineResults(string NewResults, dynamic JsonParams)
        {
            if(Results.Length < 1)
            {
                Results = NewResults;
                return;
            }

            var TotalEvals = this.Evaluations;

            dynamic PrevRes = JsonConvert.DeserializeObject(Results);
            dynamic NewRes = JsonConvert.DeserializeObject(NewResults);

            foreach (var Evaluator in JsonParams.evaluators)
            {
                if ((bool)Evaluator.enabled == true)
                {
                    string Metric = Evaluator.metric;

                    JToken MetricTypeT = JsonParams.SelectToken("$.metrics[?(@.name == '" + Metric + "')]");

                    string MetricType = MetricTypeT.Value<string>("type"); 
                    if (MetricType == "List")
                    {
                        foreach(var X in NewRes.metrics[Metric])
                        {
                            PrevRes.metrics[Metric].Add(X);
                        }
                    }

                    if (MetricType == "Double")
                    {
                        PrevRes.metrics[Metric] = ((double)PrevRes.metrics[Metric] * ((double)(TotalEvals - 1) / TotalEvals)) + ((double)NewRes.metrics[Metric] / TotalEvals);
                    }
                }
            }

            Results = PrevRes.ToString();
        }

        public static List<double> EvaluateResults(dynamic JsonParams, string Results, List<double> Vector)
        {
            List<double> finalResults = new List<double>();

            dynamic JResults = JsonConvert.DeserializeObject(Results);

            //int index = 0;
            foreach (var Evaluator in JsonParams.evaluators)
            {
                if ((bool)Evaluator.enabled == true)
                {
                    string EvalType = "GeneticAlgorithm.Evaluators." + Evaluator.type + ", GeneticAlgorithm";
                    string Metric = Evaluator.metric;
                    double Target = Evaluator.target;
                    double Weight = Evaluator.weight;
                    double OptionalParam = Evaluator.optionalparam != null ? Evaluator.optionalparam : 0;

                    MetricEvaluator Eval = null;

                    Eval = (MetricEvaluator)Activator.CreateInstance(Type.GetType(EvalType), new object[] { });

                    double EvalScore = 0;

                    JToken MetricTypeT = JsonParams.SelectToken("$.metrics[?(@.name == '" + Metric + "')]");

                    string MetricType = MetricTypeT.Value<string>("type"); 
                    if (MetricType == "List")
                    {
                        EvalScore = Eval.Evaluate(JResults.metrics[Metric].ToObject<List<double>>(), Target, OptionalParam);
                    }

                    if (MetricType == "Double")
                    {
                        EvalScore = Eval.Evaluate(JResults.metrics[Metric].ToObject<double>(), Target, OptionalParam);
                    }

                    finalResults.Add(EvalScore * Weight);
                }
            }

            dynamic Params = JsonParams.parameters;

            int x = 0;
            foreach (var P in Params)
            {
                if ((bool)P.enabled == true && (string)P.minimise != "ignore")
                {
                    double ParamFitness = 0;

                    int ListSize = P.listsize != null ? (int)P.listsize : 1;
                    for (int i = 0; i < ListSize; i++)
                    {
                        if ((string)P.minimise == "minimise")
                        {
                            ParamFitness += Math.Abs(Vector[x]) * (double)P.weight;
                        }

                        if ((string)P.minimise == "maximise")
                        {
                            //TODO
                        }

                        x++;
                    }

                    finalResults.Add(ParamFitness);
                } else
                {
                    x += P.listsize != null ? (int)P.listsize : 1;
                }


            }

            return finalResults;
        }

        public static string RunGamesLocal(dynamic JsonParams, List<double> Vector, int RandSeed)
        {
            try
            {
                var Simulator = new Process();

                //string Invoke = (string)Manager.GetParameters().GetParameter("string_Bridge_Local_Exe");
                string Invoke = (string)JsonParams.bridge.executable;

                string EXE = Invoke.Substring(0, Invoke.IndexOf(' '));
                string ARGS = Invoke.Substring(Invoke.IndexOf(' '));

                //TODO FIX
                //ARGS = ARGS.Replace("%0", (string)Manager.GetParameters().GetParameter("string_SpecsFile"));
                ARGS = ARGS.Replace("%1", RandSeed.ToString());
                ARGS = ARGS.Replace("%2", string.Join(",", Vector)); //TODO also check for Null vector

                ProcessStartInfo info = new ProcessStartInfo(EXE, ARGS);
                //info.WorkingDirectory = CompetDirectory;
                info.UseShellExecute = false;
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;

                Simulator.StartInfo = info;

                Simulator.Start();

                //Run stuff
                string AllOutput = "";
                while (!Simulator.StandardOutput.EndOfStream || !Simulator.HasExited)
                {
                    AllOutput += Simulator.StandardOutput.ReadLine();
                }

                int pFrom = AllOutput.IndexOf("BEGIN METRICS") + "BEGIN METRICS".Length;
                int pTo = AllOutput.IndexOf("END METRICS");
                AllOutput = AllOutput.Substring(pFrom, pTo - pFrom);

                Simulator.Close();

                //Simulator.Kill();

                return AllOutput;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string RunGamesRemote(dynamic JsonParams, List<double> Vector, int RandSeed)
        {
            try
            {
                //dynamic JsonMessage = new JObject(Manager.GetParameters().JsonParams);
                dynamic JsonMessage = new JObject(JsonParams);

                int x = 0;
                foreach (var Param in JsonMessage.parameters)
                {
                    if ((bool)Param.enabled == true)
                    {
                        int ListSize = Param.listsize != null ? (int)Param.listsize : 1;

                        if(ListSize == 1)
                        {
                            Param.Add("value", Vector != null ? Vector[x] : 0);
                        } else
                        {
                            Param.Add("value", Vector != null ? new JArray(Vector.GetRange(x, ListSize)) : new JArray(new double[ListSize]));
                        }

                        x+=ListSize;
                    }
                }

                JsonMessage.Add("randomseed", RandSeed);

                RPCClient rpcClient = null;

                var P = JsonParams.bridge;
                while (rpcClient == null)
                {
                    try
                    {
                        rpcClient = new RPCClient((string)P.queuename, (string)P.server, (int)P.port, (string)P.username, (string)P.password, (string)P.amqpurl);
                    }
                    catch
                    {
                        rpcClient = null;
                    }
                }

                var response = rpcClient.CallRunGame(JsonMessage.ToString());

                rpcClient.Close();

                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        bool IsDistinctPresent()
        {
            bool DistinctIsPresent = false;

            dynamic Params = Manager.GetParameters().JsonParams.parameters;

            foreach (var P in Params)
            {
                if ((bool)P.enabled == true)
                {
                    int ListSize = P.listsize != null ? (int)P.listsize : 1;

                    if (ListSize > 1 && P.distinct != null && (bool)P.distinct == true)
                    {
                        DistinctIsPresent = true;
                    }
                }
            }

            return DistinctIsPresent;
        }

        public override T Crossover<T>(T b)
        {
            BalanceGA CWith = ((BalanceGA)(object)b);

            var newData = new List<double>(Vector);

            int Length = Vector.Count;

            if (!IsDistinctPresent())
            {
                if (Length == 1)
                {
                    newData[0] = (Vector[0] + CWith.Vector[0]) / 2;
                }
                else
                {

                    int p1 = rand.Next(Length - 1);
                    int p2 = rand.Next(p1 + 1, Length);

                    for (int i = p1; i < p2; i++)
                    {
                        newData[i] = CWith.Vector[i];
                    }
                }
            } else
            {
                //For now, distinct only works when there is a single parameter list
                var newSet = new HashSet<double>();
                foreach (var X in Vector)
                    newSet.Add(X);
                foreach (var X in CWith.Vector)
                    newSet.Add(X);

                var vec = newSet.ToList();
                vec.Shuffle();
                
                newData = vec.Take(Length).ToList();
            }

            PopulationMember ret = (T)Activator.CreateInstance(typeof(T), new object[] { Manager, newData, rand });

            ((BalanceGA)ret).CreatedBy = "Crossover";

            return (T)ret;
        }

        public override T Mutate<T>()
        {
            var newData = new List<double>(Vector);

            double MutationChance = Manager.GetParameters().GetParameter("extra_MutationChance", 0.5);

            dynamic Params = Manager.GetParameters().JsonParams.parameters;

            int x = 0;
            foreach (var P in Params)
            {
                if ((bool)P.enabled == true)
                {
                    int ListSize = P.listsize != null ? (int)P.listsize : 1;

                    double MutateRange = (double)((double)P.rangeMax - (double)P.rangeMin) / (double)P.rangeAccuracy;

                    if (P.distinct == null || (bool)P.distinct == false)
                    {
                        for (int i = 0; i < ListSize; i++)
                        {
                            if (rand.Next(0, 100) <= MutationChance * 100)
                            {
                                /*double Shift = Math.Round(rand.NextDouble(-MutateRange, MutateRange), (int)Math.Log10((double)P.rangeAccuracy));
                                newData[x] += Shift;
                                if (newData[x] > (double)P.rangeMax)
                                    newData[x] = (double)P.rangeMax;
                                if (newData[x] < (double)P.rangeMin)
                                    newData[x] = (double)P.rangeMin;*/
                                newData[x] = GetValInRange((double)P.rangeMin, (double)P.rangeMax, (int)P.rangeAccuracy);
                            }
                            x++;
                        }
                    } else
                    {
                        //Only works with 1 parameter
                        var Allowed = new List<double>();
                        for(int i=(int)P.rangeMin; i<=(int)P.rangeMax; i++)
                        {
                            Allowed.Add(i);
                        }
                        foreach(var X in Vector)
                        {
                            Allowed.Remove(X);
                        }

                        for (int i = 0; i < ListSize; i++)
                        {
                            if (rand.Next(0, 100) <= MutationChance * 100)
                            {
                                double temp = newData[x];
                                newData[x] = Allowed[rand.Next(0, Allowed.Count)];
                                Allowed.Remove(newData[x]);
                                Allowed.Add(temp);
                            }
                            x++;
                        }
                    }
                }
            }

            PopulationMember ret = new BalanceGA(Manager, newData, rand);

            ((BalanceGA)ret).CreatedBy = "Mutation";

            return (T)ret;
        }

        public override string ToString()
        {
            return string.Join(",", Vector);
        }

        public override PopulationMember Clone()
        {
            BalanceGA ret = new BalanceGA(Manager, new List<double>(Vector), rand);
            return ret;
        }

        public override double GetFitness()
        {
            return this.Fitness;
        }

        public override PopulationManager<T> GetParentManager<T>()
        {
            return Manager as PopulationManager<T>;
        }
    }
}
