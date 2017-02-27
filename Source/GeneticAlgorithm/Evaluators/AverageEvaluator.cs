using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Evaluators
{
    public class AverageEvaluator : MetricEvaluator
    {
        public double Evaluate(List<double> Metric, double Target)
        {
            return Math.Abs(Metric.Average() - Target);
        }

        public double Evaluate(double Metric, double Target)
        {
            return Math.Abs(Metric - Target);
        }
    }
}
