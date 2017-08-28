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
    public class AnalysisTool
    {
        public MainAnalysisObject MA;

        public int X { get; set; }
        public int Y { get; set; }

        public AnalysisTool(MainAnalysisObject MA)
        {
            this.MA = MA;
            this.X = 0;
            this.Y = 1;
        }

        public virtual ChartValues<ObservablePoint> SeriesData
        {
            get;
        }

        public virtual ChartValues<HeatPoint> SeriesHeatMap
        {
            get;
        }

        public string SeriesDataJSON
        {
            get
            {
                var PF = this.SeriesData;

                string json = JsonConvert.SerializeObject(PF, Formatting.Indented);

                return json;
            }
        }
    }
}
