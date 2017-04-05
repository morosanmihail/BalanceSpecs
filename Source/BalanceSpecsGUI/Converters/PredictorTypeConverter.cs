using SharpGenetics.Predictor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace BalanceSpecsGUI.Converters
{
    public class NameValuePair
    {
        public dynamic Value { get; set; }
        public string Name { get; set; }
        public NameValuePair(string N, dynamic V)
        {
            Name = N;
            Value = V;
        }
    }
    
    public class PredictorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null)
                return null;

            if (values[0] is string)
            {
                var ParamNames = PredictorHelper.GetParametersRequired((string)values[0]);
                List<NameValuePair> JObjects = new List<NameValuePair>();

                foreach(var ParamName in ParamNames)
                {
                    if (MainWindow.JSONFile.gaparams[ParamName] == null)
                    {
                        //Add it first
                        MainWindow.JSONFile.gaparams[ParamName] = "0";
                    } 
                    JObjects.Add(new NameValuePair(ParamName, MainWindow.JSONFile.gaparams[ParamName]));
                }

                return JObjects;
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
