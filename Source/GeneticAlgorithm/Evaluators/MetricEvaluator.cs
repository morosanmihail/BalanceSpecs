using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.Evaluators
{
    public interface MetricEvaluator
    {
        double Evaluate(List<double> Metric, double Target, double OptionalParam = 0);
        double Evaluate(double Metric, double Target, double OptionalParam = 0);
    }
}
