using GeneticAlgorithm.AnalysisTools;
using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BalanceSpecsGUI.Windows.Analysis
{
    [ImplementPropertyChanged]
    public class AnalysisOfRunDataContext
    {
        public ObservableCollection<MainAnalysisObject> MAS { get; set; }

        public SeriesCollection AllSeries
        {
            get
            {
                var A = new SeriesCollection();

                foreach(var M in MAS)
                {
                    M.SelectedAnalysisTool = SelectedAnalysisTool;
                    A.Add(new LineSeries { Title = M.Folder, Values = M.SelectedSeries });
                }

                return A;
            }
        }

        public string SelectedAnalysisTool { get; set; }

        public AnalysisOfRunDataContext()
        {
            MAS = new ObservableCollection<MainAnalysisObject>();

            SelectedAnalysisTool = "ParetoFrontAnalysis";
        }

        public void AddAnalysisObject(MainAnalysisObject ma)
        {
            MAS.Add(ma);
        }
    }

    /// <summary>
    /// Interaction logic for AnalysisOfRun.xaml
    /// </summary>
    public partial class AnalysisOfRun : MetroWindow
    {
        AnalysisOfRunDataContext DC = new AnalysisOfRunDataContext();

        MainWindow Parent;

        public AnalysisOfRun(string Folder, MainWindow ParentWindow)
        {
            InitializeComponent();

            Parent = ParentWindow;

            this.DataContext = DC;

            AddFolder(Folder);
        }

        public void AddFolder(string Folder)
        {
            MainAnalysisObject ma = new MainAnalysisObject();
            ma.Initialise(Folder);

            DC.AddAnalysisObject(ma);

            MainChart.GetBindingExpression(CartesianChart.SeriesProperty).UpdateTarget();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Parent.AnalysisWindow = null;
        }
    }
}
