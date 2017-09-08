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
    /// Interaction logic for Metrics.xaml
    /// </summary>
    public partial class Metrics : UserControl
    {
        public Metrics()
        {
            InitializeComponent();
        }

        private async void AddMetricButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            if (NewMetricName.Text.Trim().Length < 1)
            {
                var window = Window.GetWindow(this) as MahApps.Metro.Controls.MetroWindow;
                await window.ShowMessageAsync("Error", "Empty names are not allowed!");
                return;
            }

            try
            {
                JsonO.Add(new JObject(new JProperty("name", NewMetricName.Text), new JProperty("type", "Double")));
            }
            catch (Exception ex)
            {
                var window = Window.GetWindow(this) as MahApps.Metro.Controls.MetroWindow;
                await window.ShowMessageAsync("Error", "A metric with the same name already exists!");
            } finally
            {
                NewMetricName.Text = "";
            }
        }

        private void RemoveMetricButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            try
            {
                JsonO.RemoveAt(MetricsList.SelectedIndex);
            }
            catch { }
        }
    }
}
