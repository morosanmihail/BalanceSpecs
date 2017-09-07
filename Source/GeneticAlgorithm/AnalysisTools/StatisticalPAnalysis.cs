using Accord.Statistics.Testing;
using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.AnalysisTools
{
    public class StatisticalPAnalysis : AnalysisTool
    {
        public StatisticalPAnalysis(MainAnalysisObject MA) : base(MA)
        {

        }

        public static ChartValues<ObservablePoint> PValues(ObservableCollection<MainAnalysisObject> MAS)
        {
            if (MAS == null || MAS.Count < 2)
            {
                return null;
            }

            var GraphA = MAS[0];
            var GraphB = MAS[1];

            if (GraphA.GACs.Count != GraphB.GACs.Count)
            {
                return null;
            }

            var res = new ChartValues<ObservablePoint>();

            //Get MaxRange
            double MaxEvals = double.PositiveInfinity;

            foreach(var M in MAS)
            {
                MaxEvals = Math.Min(M.GetSeries("ParetoFrontAnalysis").Last().X, MaxEvals);
            }

            double Step = Math.Max(MaxEvals / 100, 1);


            for (double Evals = Step; Evals <= MaxEvals; Evals += Step)
            {
                var PerRunFitnessesA = new List<double>();
                var PerRunFitnessesB = new List<double>();

                for (int i=0;i<GraphA.GACs.Count;i++)
                {
                    bool AddedToA = false;
                    bool AddedToB = false;
                    var PFA = ParetoFrontAnalysis.GetParetoFront(GraphA.GACs[i]);
                    var PFB = ParetoFrontAnalysis.GetParetoFront(GraphB.GACs[i]);

                    foreach(var Val in PFA)
                    {
                        if(Val.X > Evals)
                        {
                            PerRunFitnessesA.Add(Val.Y);
                            AddedToA = true;
                            break;
                        }
                    }
                    if(!AddedToA)
                    {
                        PerRunFitnessesA.Add(PFA.Last().Y);
                    }

                    foreach (var Val in PFB)
                    {
                        if (Val.X > Evals)
                        {
                            PerRunFitnessesB.Add(Val.Y);
                            AddedToB = true;
                            break;
                        }
                    }
                    if(!AddedToB)
                    {
                        PerRunFitnessesB.Add(PFB.Last().Y);
                    }
                }

                if(PerRunFitnessesA.Except(PerRunFitnessesB).Count() == 0)
                {
                    res.Add(new ObservablePoint(Evals, 0.5));
                } else
                {
                    TwoSampleHypothesis Hypo = TwoSampleHypothesis.FirstValueIsGreaterThanSecond;
                    var WilcoxonTest = new TwoSampleWilcoxonSignedRankTest(PerRunFitnessesA.ToArray(), PerRunFitnessesB.ToArray(), Hypo);

                    res.Add(new ObservablePoint(Evals, WilcoxonTest.PValue));
                }

            }

            return res;
        }
    }
}
