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
    /// Interaction logic for GameParameters.xaml
    /// </summary>
    public partial class GameParameters : UserControl
    {
        public GameParameters()
        {
            InitializeComponent();
        }

        private async void AddParameterButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            string Name = NewParameterName.Text.Trim();

            if (Name.Length < 1)
            {
                var window = Window.GetWindow(this) as MahApps.Metro.Controls.MetroWindow;
                await window.ShowMessageAsync("Error", "Empty names are not allowed!");
                return;
            }

            JsonO.Add(new JObject(new JProperty("name", Name), new JProperty("enabled", true), new JProperty("distinct",false), new JProperty("listsize", 1), new JProperty("minimise", "ignore"), new JProperty("custom", new JObject())));
        }

        private void AddParamCustomButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = (sender as FrameworkElement).DataContext;
            if (JsonO != null)
            {
                JsonO[NewParamCustomData.Text] = "";
            }

            NewParamCustomData.Text = "";
        }

        private void RemoveParamCustomButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = (sender as FrameworkElement).DataContext;
            if (JsonO != null)
            {
                dynamic Selected = CustomProperties.SelectedItem;

                JsonO.Remove(Selected.Name);

                //JsonO[Selected.Name] = null;
            }
        }
    }
}
