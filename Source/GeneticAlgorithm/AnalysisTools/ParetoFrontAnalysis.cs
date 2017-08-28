using LiveCharts;
using LiveCharts.Defaults;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.AnalysisTools
{
    public class ParetoFrontAnalysis : AnalysisTool
    {
        public ParetoFrontAnalysis(MainAnalysisObject MA) : base(MA)
        {

        }

        public ChartValues<ObservablePoint> GetParetoFront(GAController.GAController GAC)
        {
            var RunManager = GAC.RunManager;
            if (RunManager != null && RunManager.Populations.Count > 0)
            {
                var X = new ChartValues<ObservablePoint>();
                for (int i = 0; i < RunManager.Populations[0].RunMetrics.BestFitnesses.Count; i++)
                {
                    X.Add(new ObservablePoint(RunManager.Populations[0].RunMetrics.TotalFitnessCalculations[i].Value, RunManager.Populations[0].RunMetrics.BestFitnesses[i].Value));
                }
                return X;
            }
            else
            {
                return null;
            }
        }

        public override ChartValues<ObservablePoint> SeriesData
        {
            get
            {
                if (MA.GACs != null && MA.GACs.Count > 0)
                {
                    var res = new ChartValues<ObservablePoint>();

                    List<double> AvgFitness = new List<double>();
                    List<double> Evals = new List<double>();

                    int Count = GetParetoFront(MA.GACs[0]).Count; //MA.GACs[0].ParetoFront2.Count;

                    foreach (var G in MA.GACs)
                    {
                        Count = Math.Min(Count, GetParetoFront(G).Count);
                    }

                    for (int i = 0; i < Count; i++)
                    {
                        res.Add(new ObservablePoint(0, 0));
                    }

                    foreach (var G in MA.GACs)
                    {
                        var PF = GetParetoFront(G);// G.ParetoFront2;
                        for (int i = 0; i < Count; i++)
                        {
                            res[i].X += PF[i].X;
                            res[i].Y += PF[i].Y;
                        }
                    }

                    for (int i = 0; i < Count; i++)
                    {
                        res[i].X /= MA.GACs.Count;
                        res[i].Y /= MA.GACs.Count;
                    }

                    return res;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
