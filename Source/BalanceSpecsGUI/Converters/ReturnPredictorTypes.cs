using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpGenetics.Predictor;

namespace BalanceSpecsGUI.Converters
{
    public static class ReturnPredictorTypes
    {
        /*public static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, Assembly assembly)
        {
            return from x in assembly.GetTypes()
                   from z in x.GetInterfaces()
                   let y = x.BaseType
                   where
                   (y != null && y.IsGenericType &&
                   openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                   (z.IsGenericType &&
                   openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
                   select x;
        }*/

        public static List<string> GetTypes()
        {
            /*var list = GetAllTypesImplementingOpenGenericType(typeof(ResultPredictor<,>), Assembly.GetAssembly(typeof(ResultPredictor<,>)));

            var list2 = list.Select(t => t.Name).ToList();

            return list2;*/
            Assembly cA = Assembly.GetAssembly(typeof(ResultPredictor));

            var list = cA.GetTypes();

            var list2 = list.Where(t => t.IsSubclassOf(typeof(ResultPredictor)) && !t.IsAbstract && t.IsPublic).ToList();
            //var list2 = list.Where(t => t.GetInterfaces().Contains(typeof(AnalysisTool)) && !t.IsAbstract && t.IsPublic).ToList();

            var list3 = list2.Select(p => p.Name).ToList();

            return list3;
        }
    }
}
