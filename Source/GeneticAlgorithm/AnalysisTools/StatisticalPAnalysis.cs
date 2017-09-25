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
        public StatisticalPAnalysis()
        {

        }
        
        public override List<Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>> GetSeries(List<MainAnalysisObject> MAS)
        {
            var Res = new List<Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>>();
            
            var T = StatisticalPAnalysis.PValues(MAS);
            Res.Add(new Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>(new Tuple<string, int>("PValues", 1), T));

            var T2 = StatisticalPAnalysis.PValues(MAS, Accord.Statistics.Testing.TwoSampleHypothesis.FirstValueIsSmallerThanSecond);
            Res.Add(new Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>(new Tuple<string, int>("PValuesWorse", 1), T2));

            if (T == null)
                return Res;


            Res.Add(new Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>(new Tuple<string, int>("First", 0), ParetoFrontAnalysis.GetParetoFrontsAverage(MAS[0].GACs.ToList())));

            Res.Add(new Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>(new Tuple<string, int>("Second", 0), ParetoFrontAnalysis.GetParetoFrontsAverage(MAS[1].GACs.ToList())));


            var SigLine = new ChartValues<ObservablePoint>() { new ObservablePoint(0, 0.05), new ObservablePoint(T.Last().X, 0.05) };

            Res.Add(new Tuple<Tuple<string, int>, ChartValues<ObservablePoint>>(new Tuple<string, int>("Significance", 1), SigLine));

            return Res;
        }

        public static ChartValues<ObservablePoint> PValues(List<MainAnalysisObject> MAS, TwoSampleHypothesis Hypo = TwoSampleHypothesis.FirstValueIsGreaterThanSecond)
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
            double MinEvals = double.PositiveInfinity;

            foreach(var M in MAS)
            {
                MaxEvals = Math.Min(M.GetSeries("ParetoFrontAnalysis").Last().X, MaxEvals);
                MinEvals = Math.Min(M.GetSeries("ParetoFrontAnalysis").First().X, MinEvals);
            }

            double Step = Math.Max((MaxEvals - MinEvals) / 100, 1);


            for (double Evals = MinEvals; Evals <= MaxEvals; Evals += Step)
            {
                var PerRunFitnessesA = new List<double>();
                var PerRunFitnessesB = new List<double>();

                for (int i=0;i<GraphA.GACs.Count;i++)
                {
                    bool AddedToA = false;
                    bool AddedToB = false;
                    var PFA = ParetoFrontAnalysis.GetParetoFront(GraphA.GACs[i]);
                    var PFB = ParetoFrontAnalysis.GetParetoFront(GraphB.GACs[i]);

                    double ValY = -1;

                    /*foreach(var Val in PFA)
                    {
                        if(Val.X > Evals)
                        {
                            PerRunFitnessesA.Add(Val.Y);
                            AddedToA = true;
                            break;
                        }
                    }*/
                    foreach(var Val in PFA)
                    {
                        if(Val.X <= Evals)
                        {
                            ValY = Val.Y;
                            AddedToA = true;
                        } else
                        {
                            break;
                        }
                    }
                    if(!AddedToA)
                    {
                        PerRunFitnessesA.Add(PFA.Last().Y);
                    } else
                    {
                        PerRunFitnessesA.Add(ValY);
                    }

                    /*foreach (var Val in PFB)
                    {
                        if (Val.X > Evals)
                        {
                            PerRunFitnessesB.Add(Val.Y);
                            AddedToB = true;
                            break;
                        }
                    }*/
                    foreach (var Val in PFB)
                    {
                        if (Val.X <= Evals)
                        {
                            ValY = Val.Y;
                            AddedToB = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (!AddedToB)
                    {
                        PerRunFitnessesB.Add(PFB.Last().Y);
                    }
                    else
                    {
                        PerRunFitnessesB.Add(ValY);
                    }
                }

                if(PerRunFitnessesA.Except(PerRunFitnessesB).Count() == 0)
                {
                    res.Add(new ObservablePoint(Evals, 0.5));
                } else
                {
                    var WilcoxonTest = new TwoSampleWilcoxonSignedRankTest(PerRunFitnessesA.ToArray(), PerRunFitnessesB.ToArray(), Hypo);

                    res.Add(new ObservablePoint(Evals, WilcoxonTest.PValue));
                }

            }

            return res;
        }
    }
}
