using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BalanceSpecsGUI.Converters
{
    public class ConnectionTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = System.Convert.ToString(value).ToLower();
            if (string.IsNullOrWhiteSpace(val))
                return false;

            return val == "remote" ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = System.Convert.ToBoolean(value);

            return val ? "remote" : "local";
        }
    }
}
