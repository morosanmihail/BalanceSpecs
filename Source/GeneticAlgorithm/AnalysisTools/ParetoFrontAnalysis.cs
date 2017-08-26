﻿using LiveCharts;
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

        public override ChartValues<ObservablePoint> SeriesData
        {
            get
            {
                if (MA.GACs != null && MA.GACs.Count > 0)
                {
                    var res = new ChartValues<ObservablePoint>();

                    List<double> AvgFitness = new List<double>();
                    List<double> Evals = new List<double>();

                    int Count = MA.GACs[0].ParetoFront2.Count;

                    foreach (var G in MA.GACs)
                    {
                        if (G.ParetoFront2.Count < Count)
                        {
                            Count = G.ParetoFront2.Count;
                        }
                    }

                    for (int i = 0; i < Count; i++)
                    {
                        res.Add(new ObservablePoint(0, 0));
                    }

                    foreach (var G in MA.GACs)
                    {
                        var PF = G.ParetoFront2;
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
