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
        public AnalysisTool(MainAnalysisObject MA)
        {
            this.MA = MA;
        }

        public virtual ChartValues<ObservablePoint> SeriesData
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
