using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using MahApps.Metro.Controls.Dialogs;

namespace BalanceSpecsGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            JObject JsonO = JObject.Parse(File.ReadAllText(@"F:\GitHub\PacMan-CSharp\Bridges\MsPacman.json"));

            this.DataContext = JsonO;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            MessageBox.Show(JsonO.evaluators.ToString());
        }

        private void AddParameterButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            JsonO.parameters.Add(new JObject(new JProperty("name","Something"), new JProperty("rangeMax",4)));
        }

        private async void AddMetricButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;
            
            if(NewMetricName.Text.Trim().Length < 1)
            {
                await this.ShowMessageAsync("Error", "Empty names are not allowed!");
                return;
            }

            try
            {
                JsonO.metrics.Add(new JObject(new JProperty("name", NewMetricName.Text), new JProperty("type", "Double")));
            } catch(Exception ex)
            {
                await this.ShowMessageAsync("Error", "A metric with the same name already exists!");
            }
        }

        private void AddCustomButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            JsonO.custom[NewCustomData.Text] = "";
        }
    }
}
