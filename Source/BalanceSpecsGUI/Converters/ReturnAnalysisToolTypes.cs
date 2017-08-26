using GeneticAlgorithm.AnalysisTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BalanceSpecsGUI.Converters
{
    public static class ReturnAnalysisToolTypes
    {
        public static List<string> GetTypes()
        {
            Assembly cA = Assembly.GetAssembly(typeof(AnalysisTool));

            var list = cA.GetTypes();

            var list2 = list.Where(t => t.IsSubclassOf(typeof(AnalysisTool)) && !t.IsAbstract && t.IsPublic).ToList();
            //var list2 = list.Where(t => t.GetInterfaces().Contains(typeof(AnalysisTool)) && !t.IsAbstract && t.IsPublic).ToList();

            var list3 = list2.Select(p => p.Name).ToList();

            return list3;
        }
    }
}
