using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Evaluators
{
    public class StandardDeviationEvaluator : MetricEvaluator
    {
        public double Evaluate(List<double> Metric, double Target)
        {
            double ret = 0;
            if (Metric.Count() > 0)
            {
                //Compute the Average      
                double avg = Metric.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = Metric.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (Metric.Count() - 1));
            }
            return Math.Abs(ret - Target);
        }

        public double Evaluate(double Metric, double Target)
        {
            return Math.Abs(Metric - Target);
        }
    }
}
