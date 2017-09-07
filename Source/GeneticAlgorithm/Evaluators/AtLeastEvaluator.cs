using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Evaluators
{
    public class AtLeastProportionEvaluator : MetricEvaluator
    {
        public double Evaluate(List<double> Metric, double Target, double OptionalParam = 0)
        {
            var CountOver = (double)Metric.Count(m => m >= OptionalParam) / (double)Metric.Count;

            if(CountOver >= Target)
            {
                return 0;
            }

            return Math.Abs(Target - (CountOver));
        }

        public double Evaluate(double Metric, double Target, double OptionalParam = 0)
        {
            return Math.Abs(Target - (Metric >= OptionalParam ? 1 : 0));
        }
    }
}
