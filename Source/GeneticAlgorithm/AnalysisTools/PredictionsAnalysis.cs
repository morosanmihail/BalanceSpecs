using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.AnalysisTools
{
    public class PredictionsAnalysis : AnalysisTool
    {
        public PredictionsAnalysis()
        {

        }

        public override List<Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>> GetSeries(List<MainAnalysisObject> MAS)
        {
            var Res = new List<Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>>();

            foreach (var M in MAS)
            {
                var res = new ChartValues<ObservablePoint>();
                double AveragePredictionsPerRun = 0;
                double FalseNegativesPerRun = 0;
                foreach(var G in M.GACs)
                {
                    AveragePredictionsPerRun += G.RunManager.Populations[0].Predictor != null ? G.RunManager.Populations[0].Predictor.AcceptedPredictions : 0;
                    FalseNegativesPerRun += G.RunManager.Populations[0].Predictor != null ? G.RunManager.Populations[0].Predictor.FalseNegativesByGeneration.Sum() : 0;
                }
                res.Add(new ObservablePoint(0, AveragePredictionsPerRun / M.GACs.Count));
                res.Add(new ObservablePoint(1, FalseNegativesPerRun / M.GACs.Count));

                Res.Add(new Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>(new Tuple<string, int>(M.Folder, 0), res));
            }

            return Res;
        }
    }
}
