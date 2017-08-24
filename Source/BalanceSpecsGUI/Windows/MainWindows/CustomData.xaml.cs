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
    /// Interaction logic for CustomData.xaml
    /// </summary>
    public partial class CustomData : UserControl
    {
        public CustomData()
        {
            InitializeComponent();
        }

        private void AddCustomButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            JsonO[NewCustomData.Text] = "";
        }
    }
}
