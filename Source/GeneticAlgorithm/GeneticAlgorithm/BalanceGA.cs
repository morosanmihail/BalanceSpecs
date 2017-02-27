﻿using GeneticAlgorithm.Evaluators;
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

namespace GeneticAlgorithm.GeneticAlgorithm
{
    [DataContract]
    class BalanceGA : PopulationMember
    {
        PopulationManager<BalanceGA, int, double> Manager;

        [DataMember]
        public String CreatedBy = "";

        [DataMember]
        public string Results = "";

        public BalanceGA(PopulationManager<BalanceGA, int, double> Manager, List<double> root = null, CRandom rand = null)
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
        }

        public override void ReloadParameters<T, I, O>(PopulationManager<T, I, O> Manager)
        {
            this.Manager = Manager as PopulationManager<BalanceGA, int, double>;
        }

        List<double> GenerateData()
        {
            int Length = (int)(double)Manager.GetParameters().GetParameter("Par_Length");
            List<double> data = new List<double>();

            List<double> RangeMax = (List<double>)Manager.GetParameters().GetParameter("Params_MaxRange");
            List<double> RangeMin = (List<double>)Manager.GetParameters().GetParameter("Params_MinRange");
            List<int> Accuracy = (List<int>)Manager.GetParameters().GetParameter("Params_Accuracy");

            for (int i = 0; i < Length; i++)
            {
                data.Add(GetValInRange(RangeMin[i], RangeMax[i], Accuracy[i]));
            }

            return data;
        }

        double GetValInRange(double ValueRangeMin, double ValueRangeMax, int ValueRangeAccuracy)
        {
            return Math.Round(rand.Next((int)(ValueRangeMin * ValueRangeAccuracy), (int)(ValueRangeMax * ValueRangeAccuracy)) / (float)ValueRangeAccuracy, (int)Math.Log10(ValueRangeAccuracy));
        }

        public override double CalculateFitness<T, Y>(int CurrentGeneration, params GenericTest<T, Y>[] values)
        {
            int RecalculateAfterAGeneration = 0; //TODO
            if (this.Fitness < 0 || RecalculateAfterAGeneration > 0)
            {
                if (Fitness < 0)
                {
                    this.CreatedAtGeneration = CurrentGeneration;
                }

                if (this.UpdatedAtGeneration < CurrentGeneration)
                {
                    Fitness = RunGames(CurrentGeneration);
                    this.UpdatedAtGeneration = CurrentGeneration;
                }
            }

            return this.Fitness;
        }

        double RunGames(int CurrentGeneration)
        {
            double finalResult = 0;

            dynamic JResults = null;
            
            if((string)Manager.GetParameters().GetParameter("string_Bridge_Type") == "local")
            {
                Results = RunGamesLocal();
            } else
            {
                var P = Manager.GetParameters().JsonParams.bridge;
                Results = RunGamesRemote((string)P.server,(int)P.port,(string)P.username,(string)P.password);
            }

            JResults = JsonConvert.DeserializeObject(Results);

            //Do stuff with Results
            //Go through evaluators
            //Get metric being targeted from Results
            //Apply evaluator type to results and desired value
            //Add result, with weight, to finalResults

            foreach (var Evaluator in Manager.GetParameters().JsonParams.evaluators)
            {
                string EvalType = Evaluator.type;
                string Metric = Evaluator.metric;
                double Target = Evaluator.target;
                double Weight = Evaluator.weight;

                MetricEvaluator Eval = null;

                Eval = (MetricEvaluator)Activator.CreateInstance(Type.GetType(EvalType), new object[] { });

                double EvalScore = 0;
                string MetricType = Manager.GetParameters().JsonParams.metrics[Metric].type;
                if (MetricType == "List")
                {
                    EvalScore = Eval.Evaluate(JResults.metrics[Metric].ToObject<List<double>>(), Target);
                }

                if (MetricType == "Double")
                {
                    EvalScore = Eval.Evaluate(JResults.metrics[Metric].ToObject<double>(), Target);
                }

                finalResult += EvalScore * Weight;

            }

            //TODO add parameter minimising / maximising

            return finalResult;
        }

        string RunGamesLocal()
        {
            try
            {
                var Simulator = new Process();

                string Invoke = (string)Manager.GetParameters().GetParameter("string_Bridge_Local_Exe");

                string EXE = Invoke.Substring(0, Invoke.IndexOf(' '));
                string ARGS = Invoke.Substring(Invoke.IndexOf(' '));

                ARGS = ARGS.Replace("%0", (string)Manager.GetParameters().GetParameter("string_SpecsFile"));
                ARGS = ARGS.Replace("%1", rand.Next().ToString());
                ARGS = ARGS.Replace("%2", string.Join(",", Vector));

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
            } catch(Exception e)
            {
                return null;
            }
        }

        string RunGamesRemote(string HostName, int Port, string Username, string Password)
        {
            try
            {
                dynamic JsonMessage = new JObject(Manager.GetParameters().JsonParams);

                int i = 0;
                foreach(var Param in JsonMessage.parameters)
                {
                    Param.Add("value", Vector[i]);
                    i++;
                }

                JsonMessage.Add("randomseed", rand.Next());

                RPCClient rpcClient = null;
                while (rpcClient == null)
                {
                    try
                    {
                        rpcClient = new RPCClient(HostName, Port, Username, Password);
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

        public override T Crossover<T>(T b)
        {
            BalanceGA CWith = ((BalanceGA)(object)b);

            var newData = new List<double>(Vector);

            int Length = Vector.Count;

            if (Length == 1)
            {
                newData[0] = (Vector[0] + CWith.Vector[0]) / 2;
            }
            else
            {

                int p1 = rand.Next(Length - 1);
                int p2 = rand.Next(p1+1, Length);

                for (int i = p1; i < p2; i++)
                {
                    newData[i] = CWith.Vector[i];
                }
            }

            PopulationMember ret = (T)Activator.CreateInstance(typeof(T), new object[] { Manager, newData, rand });

            ((BalanceGA)ret).CreatedBy = "Crossover";

            return (T)ret;
        }

        public override T Mutate<T>()
        {
            var newData = new List<double>(Vector);

            List<double> RangeMax = (List<double>)Manager.GetParameters().GetParameter("Params_MaxRange");
            List<double> RangeMin = (List<double>)Manager.GetParameters().GetParameter("Params_MinRange");
            List<int> Accuracy = (List<int>)Manager.GetParameters().GetParameter("Params_Accuracy");

            double MutationChance = (double)Manager.GetParameters().GetParameter("extra_MutationChance");
            
            for (int i = 0; i < Vector.Count; i++)
            {
                double MutateRange = (double)(RangeMax[i] - RangeMin[i]) / Accuracy[i];
                if (rand.Next(0, 100) <= MutationChance * 100)
                {
                    double Shift = Math.Round(rand.NextDouble(-MutateRange, MutateRange), (int)Math.Log10(Accuracy[i]));
                    newData[i] += Shift;
                    if (newData[i] > RangeMax[i])
                        newData[i] = RangeMax[i];
                    if (newData[i] < RangeMin[i])
                        newData[i] = RangeMin[i];
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

        public override PopulationManager<T, I, O> GetParentManager<T, I, O>()
        {
            return Manager as PopulationManager<T, I, O>;
        }
    }
}
