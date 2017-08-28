using GeneticAlgorithm.AnalysisTools;
using GeneticAlgorithm.GAController;
using LiveCharts;
using LiveCharts.Defaults;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    [AddINotifyPropertyChangedInterface]
    public class GARunDataContext
    {
        public MainAnalysisObject MA { get; set; }
        public GAController GAC { get; set; }
        
        public int HeatMapParameterIndexX { get; set; }

        public int HeatMapParameterIndexY { get; set; }

        public GARunDataContext()
        {
            HeatMapParameterIndexX = 0;
            HeatMapParameterIndexY = 1;
        }

        public ChartValues<ObservablePoint> BestFitnessEval
        {
            get
            {
                return MA.GetSeries("FitnessOverTimeAnalysis");
            }
        }

        public ChartValues<ObservablePoint> ParetoFront
        {
            get
            {
                return MA.GetSeries("ParetoFrontAnalysis");
            }
        }

        public ChartValues<HeatPoint> PredictorHeatMap
        {
            get
            {
                return MA.GetHeatMap("PredictionHeatMapAnalysis", HeatMapParameterIndexX, HeatMapParameterIndexY);
            }
        }

        public string HeatMapParameterX
        {
            get
            {
                return GAC.Parameters[HeatMapParameterIndexX].Name;
            }
        }

        public string HeatMapParameterY
        {
            get
            {
                return GAC.Parameters[HeatMapParameterIndexY].Name;
            }
        }
    }

    /// <summary>
    /// Interaction logic for GARun.xaml
    /// </summary>
    public partial class GARun : MetroWindow
    {
        //public MainAnalysisObject MA { get; set; }

        //GAController GAC;

        GARunDataContext Context { get; set; }

        public GARun(GAController Controller)
        {
            InitializeComponent();

            Context = new GARunDataContext();
            Context.GAC = Controller;

            //GAC = Controller;

            Context.MA = new MainAnalysisObject();
            Context.MA.Initialise(Controller);
            Context.MA.SelectedAnalysisTool = "ParetoFrontAnalysis";
            
            this.DataContext = Context;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (Context.GAC != null)
            {
                Context.GAC.KillRun();
            }

            base.OnClosed(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Context.GAC.StartOrPauseRun();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Context.GAC.KillRun();
        }

        private void ManualSaveButton_Click(object sender, RoutedEventArgs e)
        {
            string Filename = "";

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML file (*.xml)|*.xml";
            
            if (saveFileDialog.ShowDialog() == true)
            {
                Filename = saveFileDialog.FileName;

                GAController.SaveRunGAToFile(Context.GAC.RunManager, Filename);
            }
        }

        private void AutosaveLocationTextbox_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose Autosave location";
            dlg.IsFolderPicker = true;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsureFileExists = false;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Context.GAC.AutosaveLocation = dlg.FileName;
                Context.GAC.IsAutosaving = true;
                //AutosaveLocationTextbox.Text = dlg.FileName;
            }
        }

        private void MetroWindow_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Context.GAC != null && Context.GAC.isStarted)
            {
                MessageBoxResult result = MessageBox.Show("Run is still in progress. Are you sure you want to close?", "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
