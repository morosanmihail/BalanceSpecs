using SharpGenetics.Predictor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using Newtonsoft.Json.Linq;
using SharpGenetics.Helpers;

namespace BalanceSpecsGUI.Converters
{
    public class NameValuePair
    {
        public dynamic Value { get; set; }
        public ImportantParameterAttribute Name { get; set; }
        public NameValuePair(ImportantParameterAttribute N, dynamic V)
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
                var Params = PredictorHelper.GetParametersRequired((string)values[0]);
                List<NameValuePair> JObjects = new List<NameValuePair>();

                foreach(var Param in Params)
                {
                    var ParamName = Param.ParameterName;
                    if (MainWindow.JSONFile.gaparams[ParamName] == null)
                    {
                        //Add it first
                        MainWindow.JSONFile.gaparams[ParamName] = Param.Default;
                    } 
                    JObjects.Add(new NameValuePair(Param, MainWindow.JSONFile.gaparams[ParamName]));
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
