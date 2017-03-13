using GeneticAlgorithm.GAController;
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
    /// Interaction logic for GARun.xaml
    /// </summary>
    public partial class GARun : Window
    {
        public GARun()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var GAC = this.DataContext as GAController;

            GAC.StartOrPauseRun();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var GAC = this.DataContext as GAController;

            GAC.KillRun();
        }
    }
}
