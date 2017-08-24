using BalanceSpecsGUI.Tools;
using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace BalanceSpecsGUI.Windows
{
    /// <summary>
    /// Interaction logic for AddToolForm.xaml
    /// </summary>
    public partial class AddToolForm : MetroWindow
    {
        public AddToolForm()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExternalTool.Initialise("BalanceSpecsGUI.Resources.DefaultTools.json");

            ExternalTool.AddToolToSettings(PathToExecutable.Text, Name.Text);

            this.Close();
        }
    }
}
