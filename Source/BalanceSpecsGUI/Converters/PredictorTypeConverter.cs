using SharpGenetics.Predictor;
using System;
using System.Windows.Data;
using BalanceSpecsGUI.Windows;

namespace BalanceSpecsGUI.Converters
{ 
    public class PredictorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null)
                return null;

            if (values[0] is string)
            {
                return PredictorHelper.ApplyPredictorPropertiesToJsonDynamicAndReturnObjects(MainWindow.JSONFile, (string)values[0]);
            }
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
