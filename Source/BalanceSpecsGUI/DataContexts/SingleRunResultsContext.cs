﻿using GeneticAlgorithm.GeneticAlgorithm;
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
    public class SingleRunResultsContext
    {
        public bool Running { get; set; }

        public string Results { get; set; }

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
        private List<double> Vector;
        private double RandSeed;

        public SingleRunResultsContext(dynamic JsonParams, List<double> Vector, double RandSeed)
        {
            this.Running = false;
            this.Results = "";
            this.JsonParams = JsonParams;
            this.Vector = Vector;
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
    }
}
