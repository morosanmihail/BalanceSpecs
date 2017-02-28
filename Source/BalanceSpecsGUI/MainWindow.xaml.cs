using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace BalanceSpecsGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            //dynamic JsonO = JsonConvert.DeserializeObject("{ \"name\" : \"test\", \"list\": [1,2,3,5,6], \"parameters\" : [ {\"name\": \"A\"}, {\"name\" : \"B\"} ] }");

            JObject JsonO = JObject.Parse( //JsonConvert.DeserializeObject(
                File.ReadAllText(@"F:\GitHub\PacMan-CSharp\Bridges\MsPacman.json"));

            this.DataContext = JsonO;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            MessageBox.Show(JsonO.parameters.ToString());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            JsonO.parameters.Add(new JObject(new JProperty("name","Something"), new JProperty("rangeMax",4)));
        }
    }
}
