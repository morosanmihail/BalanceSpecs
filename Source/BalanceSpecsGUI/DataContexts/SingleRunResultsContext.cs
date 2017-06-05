using GeneticAlgorithm.GeneticAlgorithm;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BalanceSpecsGUI.DataContexts
{
    [ImplementPropertyChanged]
    public class ValueWrapper
    {
        public double Value { get; set; }

        public ValueWrapper(double V)
        {
            this.Value = V;
        }
    }

    [ImplementPropertyChanged]
    public class SingleRunResultsContext
    {
        public bool Running { get; set; }

        public string Results { get; set; }

        public List<ValueWrapper> WVector { get; set; }

        private List<double> Vector { get {
                return WVector.Select(x => x.Value).ToList();
            } }

        public string Fitnesses
        {
            get
            {
                if (Results == null || Results.Length < 3)
                    return "";
                return string.Join(",", BalanceGA.EvaluateResults(JsonParams, Results, Vector));
            }
        }

        private Thread BackgroundThread;

        private dynamic JsonParams;
        private double RandSeed;

        public SingleRunResultsContext(dynamic JsonParams, double RandSeed)
        {
            this.Running = false;
            this.Results = "";
            this.JsonParams = JsonParams;
            this.WVector = new List<ValueWrapper>();

            //Add X elements to it
            foreach(var X in EnabledParameters)
            {
                WVector.Add(new ValueWrapper(0));
            }

            this.RandSeed = RandSeed;
        }

        public void StartRun()
        {
            BackgroundThread = new Thread(new ThreadStart(ThreadCode));

            BackgroundThread.Start();
        }

        public void KillRun()
        {
            try
            {
                if (BackgroundThread != null)
                {
                    BackgroundThread.Abort();
                }
            }
            catch { }
            finally
            {
                Running = false;
                BackgroundThread = null;
            }
        }

        private void ThreadCode()
        {
            Running = true;
            Results = GeneticAlgorithm.GeneticAlgorithm.BalanceGA.RunGamesRemote(JsonParams, Vector, RandSeed);
            Running = false;
        }

        public List<string> EnabledParameters
        {
            get
            {
                if (JsonParams != null)
                {
                    var X = new List<string>();
                    foreach (var P in JsonParams.parameters)
                    {
                        if (P.enabled.Value == true)
                        {
                            X.Add((string)P.name);
                        }
                    }
                    return X;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
