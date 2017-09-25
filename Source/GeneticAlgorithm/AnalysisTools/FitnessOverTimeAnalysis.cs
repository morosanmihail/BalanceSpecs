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
        public FitnessOverTimeAnalysis()
        {

        }

        public override List<Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>> GetSeries(List<MainAnalysisObject> MAS)
        {
            var Res = new List<Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>>();

            foreach (var M in MAS)
            {
                Res.Add(new Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>(new Tuple<string, int>(M.Folder, 0), SeriesData(M)));
            }

            return Res;
        }

        private ChartValues<ObservablePoint> SeriesData(MainAnalysisObject MA)
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
