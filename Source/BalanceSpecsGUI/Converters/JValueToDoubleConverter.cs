using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BalanceSpecsGUI.Converters
{
    public class JValueToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var jv = value as JValue;
            if (jv == null)
            {
                return null;
            }
            return (double)jv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
            //return double.Parse(value.ToString());
        }
    }
}
