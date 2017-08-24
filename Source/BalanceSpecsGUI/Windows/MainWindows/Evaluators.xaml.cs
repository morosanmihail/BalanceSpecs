using BalanceSpecsGUI.Converters;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BalanceSpecsGUI.Windows.MainWindows
{
    /// <summary>
    /// Interaction logic for Evaluators.xaml
    /// </summary>
    public partial class Evaluators : UserControl
    {
        public Evaluators()
        {
            InitializeComponent();
        }

        private async void AddEvaluatorButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            if (NewEvaluatorName.Text.Trim().Length < 1)
            {
                var window = Window.GetWindow(this) as MahApps.Metro.Controls.MetroWindow;
                await window.ShowMessageAsync("Error", "Empty names are not allowed!");
                return;
            }

            try
            {
                JsonO.Add(new JObject(new JProperty("name", NewEvaluatorName.Text), new JProperty("enabled", true), new JProperty("type", ReturnEvaluatorTypes.GetTypes()[0])));
            }
            catch (Exception ex)
            {
                var window = Window.GetWindow(this) as MahApps.Metro.Controls.MetroWindow;
                await window.ShowMessageAsync("Error", "An evaluator with the same name already exists!");
            }
        }
    }
}
