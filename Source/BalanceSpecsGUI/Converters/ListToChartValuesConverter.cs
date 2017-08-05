using GeneticAlgorithm.Helpers;
using SharpGenetics.BaseClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Helpers;

namespace BalanceSpecsGUI.Converters
{
    public class ListToChartValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = (AsyncObservableCollection<MetricPoint>)value;

            if (list == null)
                return null;
            
            var X = new ChartValues<ObservableValue>();
            for (int i = 0; i < list.Count; i++)
            {
                X.Add(new ObservableValue(list[i].Value));
            }
            return X;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
