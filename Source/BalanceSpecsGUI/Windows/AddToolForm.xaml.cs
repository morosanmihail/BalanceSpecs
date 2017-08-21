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
            if(Properties.Settings.Default.Tools2 == null)
            {
                Properties.Settings.Default.Tools2 = new System.Collections.ObjectModel.ObservableCollection<ExternalTool>();
            }

            var Tool = new ExternalTool();
            Tool.PathToExe = PathToExecutable.Text;
            Tool.Name = Name.Text;

            Properties.Settings.Default.Tools2.Add(Tool);

            Properties.Settings.Default.Save();

            this.Close();
        }
    }
}
