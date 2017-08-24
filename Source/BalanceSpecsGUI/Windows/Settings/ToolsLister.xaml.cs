using BalanceSpecsGUI.Tools;
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

namespace BalanceSpecsGUI.Windows.Settings
{
    /// <summary>
    /// Interaction logic for ToolsLister.xaml
    /// </summary>
    public partial class ToolsLister : UserControl
    {
        public ToolsLister()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Tools2.Remove(ToolsInSettings.SelectedItem as ExternalTool);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var AddToToolsWindow = new AddToolForm();

            AddToToolsWindow.ShowDialog();
        }
    }
}
