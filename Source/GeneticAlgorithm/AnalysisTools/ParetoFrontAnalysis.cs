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

        public static ChartValues<ObservablePoint> GetParetoFront(GAController.GAController GAC)
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

        public static ChartValues<ObservablePoint> GetParetoFrontsAverage(List<GAController.GAController> GACS)
        {
            if(GACS == null || GACS.Count == 0)
            {
                return null;
            }

            var res = new ChartValues<ObservablePoint>();

            double MinFinalEvals = double.PositiveInfinity;
            double MinStartEvals = double.PositiveInfinity;

            foreach (var M in GACS)
            {
                MinFinalEvals = Math.Min(M.RunManager.Populations[0].RunMetrics.TotalFitnessCalculations.Last().Value, MinFinalEvals);

                MinStartEvals = Math.Min(M.RunManager.Populations[0].RunMetrics.TotalFitnessCalculations.First().Value, MinStartEvals);
            }

            double Step = Math.Max((MinFinalEvals - MinStartEvals) / 100, 1);

            var ParetoFronts = GACS.Select(g => ParetoFrontAnalysis.GetParetoFront(g)).ToList();

            for (double Evals = MinStartEvals; Evals < MinFinalEvals; Evals += Step)
            {
                double Evaluations = 0;
                double Fitness = 0;

                for (int i = 0; i < GACS.Count; i++)
                {
                    foreach (var Val in ParetoFronts[i])
                    {
                        if (Val.X > Evals)
                        {
                            Evaluations += Val.X;
                            Fitness += Val.Y;
                            break;
                        }
                    }
                }

                Evaluations /= GACS.Count;
                Fitness /= GACS.Count;

                res.Add(new ObservablePoint(Evaluations, Fitness));
            }
            
            return res;
        }

        public override ChartValues<ObservablePoint> SeriesData
        {
            get
            {
                return GetParetoFrontsAverage(MA.GACs.ToList());
            }
        }
    }
}
