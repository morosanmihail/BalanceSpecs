using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.AnalysisTools
{
    public class FitnessOverTimeAnalysis : AnalysisTool
    {
        public FitnessOverTimeAnalysis(MainAnalysisObject MA) : base(MA)
        {

        }

        public override ChartValues<ObservablePoint> SeriesData
        {
            get
            {
                if (MA.GACs != null && MA.GACs.Count > 0)
                {
                    var res = new ChartValues<ObservablePoint>();

                    int Count = MA.GACs[0].RunManager.Populations[0].RunMetrics.BestFitnesses.Count;

                    foreach (var G in MA.GACs)
                    {
                        if (G.RunManager.Populations[0].RunMetrics.BestFitnesses.Count < Count)
                        {
                            Count = G.RunManager.Populations[0].RunMetrics.BestFitnesses.Count;
                        }
                    }

                    for (int i = 0; i < Count; i++)
                    {
                        res.Add(new ObservablePoint(i, 0));
                    }

                    foreach (var G in MA.GACs)
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            res[i].Y += G.RunManager.Populations[0].RunMetrics.BestFitnesses[i].Value;
                        }
                    }

                    for (int i = 0; i < Count; i++)
                    {
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
