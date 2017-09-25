using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.AnalysisTools
{
    /*public class PredictionHeatMapAnalysis : AnalysisTool
    {
        public PredictionHeatMapAnalysis(MainAnalysisObject MA) : base(MA)
        {
            
        }

        public override ChartValues<HeatPoint> SeriesHeatMap
        {
            get
            {
                var RunManager = MA.GACs[0].RunManager;
                var JSONParams = MA.GACs[0].JSONParams;

                int HeatMapParameterIndexX = X;
                int HeatMapParameterIndexY = Y;

                if (RunManager != null && RunManager.Populations.Count > 0 && RunManager.Populations[0].UsePredictor)
                {
                    var X = new ChartValues<HeatPoint>();

                    dynamic Params = JSONParams.parameters;

                    double XMin = 0, XMax = 0;
                    double YMin = 0, YMax = 0;

                    int paramsUsed = 0;
                    int totalParams = 0;
                    foreach (var P in Params)
                    {
                        if ((bool)P.enabled == true)
                        {
                            int ListSize = P.listsize != null ? (int)P.listsize : 1;

                            if (ListSize == 1)
                            {
                                if (totalParams == HeatMapParameterIndexX)
                                {
                                    XMin = (double)P.rangeMin;
                                    XMax = (double)P.rangeMax;
                                }
                                if (totalParams == HeatMapParameterIndexY)
                                {
                                    YMin = (double)P.rangeMin;
                                    YMax = (double)P.rangeMax;
                                }

                                paramsUsed++;
                            }
                            totalParams++;
                        }
                    }


                    if (HeatMapParameterIndexX == HeatMapParameterIndexY || HeatMapParameterIndexY >= totalParams || HeatMapParameterIndexX >= totalParams)
                    {
                        return null;
                    }

                    for (double i = XMin; i < XMax; i += (XMax - XMin) / 10)
                    {
                        for (double y = YMin; y < YMax; y += (YMax - YMin) / 10)
                        {
                            var Input = new List<double>();

                            for (int w = 0; w < Math.Min(HeatMapParameterIndexX, HeatMapParameterIndexY); w++)
                            {
                                Input.Add(0);
                            }

                            if (HeatMapParameterIndexY < HeatMapParameterIndexX)
                                Input.Add(y);
                            else
                                Input.Add(i);

                            for (int w = 0; w < Math.Abs(HeatMapParameterIndexY - HeatMapParameterIndexX) - 1; w++)
                            {
                                Input.Add(0);
                            }

                            if (HeatMapParameterIndexY < HeatMapParameterIndexX)
                                Input.Add(i);
                            else
                                Input.Add(y);

                            for (int w = Math.Max(HeatMapParameterIndexY, HeatMapParameterIndexX); w < totalParams - 1; w++)
                            {
                                Input.Add(0);
                            }

                            var Prediction = RunManager.Populations[0].Predictor.Predict(Input);

                            X.Add(new HeatPoint(i, y, Prediction.Sum()));
                        }
                    }

                    if (paramsUsed < 2)
                    {
                        return null;
                    }

                    return X;
                }
                else
                {
                    return null;
                }
            }
        }
        
    }*/
}
