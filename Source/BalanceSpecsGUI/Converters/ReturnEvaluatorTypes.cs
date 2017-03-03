using GeneticAlgorithm.Evaluators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BalanceSpecsGUI.Converters
{
    public static class ReturnEvaluatorTypes
    {
        public static List<Type> GetTypes()
        {
            Assembly cA = Assembly.GetAssembly(typeof(MetricEvaluator));

            var list = cA.GetTypes();

            var list2 = list.Where(t => t.GetInterfaces().Contains(typeof(MetricEvaluator)) && !t.IsAbstract && t.IsPublic).ToList();

            return list2;
        }
    }
}
