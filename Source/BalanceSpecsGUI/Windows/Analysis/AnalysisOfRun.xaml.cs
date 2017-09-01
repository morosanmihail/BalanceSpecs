﻿using GeneticAlgorithm.AnalysisTools;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
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
    [AddINotifyPropertyChangedInterface]
    public class AnalysisOfRunDataContext
    {
        public ObservableCollection<MainAnalysisObject> MAS { get; set; }

        public SeriesCollection AllSeries
        {
            get
            {
                var A = new SeriesCollection();

                if (SelectedAnalysisTool == "StatisticalPAnalysis")
                {
                    var T = StatisticalPAnalysis.PValues(MAS);
                    A.Add(new LineSeries { Title = "PValues", Values = T, ScalesYAt = 1 });

                    if (T == null)
                        return A;

                    var ParetoFrontAnalysisA = new ParetoFrontAnalysis(MAS[0]);
                    A.Add(new LineSeries { Title = "First", Values = ParetoFrontAnalysisA.SeriesData, ScalesYAt = 0 });
                    var ParetoFrontAnalysisB = new ParetoFrontAnalysis(MAS[1]);
                    A.Add(new LineSeries { Title = "Second", Values = ParetoFrontAnalysisB.SeriesData, ScalesYAt = 0 });

                    var SigLine = new ChartValues<ObservablePoint>() { new ObservablePoint(0, 0.05), new ObservablePoint(T.Last().X, 0.05) };

                    A.Add(new LineSeries { Title = "Significance", Values = SigLine, ScalesYAt = 1 });
                    
                    return A;
                }

                foreach(var M in MAS)
                {
                    M.SelectedAnalysisTool = SelectedAnalysisTool;
                    A.Add(new LineSeries { Title = M.Folder, Values = M.SelectedSeries });
                }

                return A;
            }
        }

        /*public AxesCollection GetStatisticalAnalysisAxis
        {
            get
            {
                var A = new AxesCollection();

                A.Add(new Axis)
                var Axis1 = new Axis();


                return A;
            }
        }*/

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

        public AnalysisOfRun()
        {
            InitializeComponent();

            this.DataContext = DC;
        }

        public void AddFolder()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose Folder";
            dlg.IsFolderPicker = true;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;
            //dlg.Filters.Add(new CommonFileDialogFilter("Balance File Format", "xml"));

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                MainAnalysisObject ma = new MainAnalysisObject();
                ma.Initialise(dlg.FileName);

                DC.AddAnalysisObject(ma);

                MainChart.GetBindingExpression(CartesianChart.SeriesProperty).UpdateTarget();
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddFolder();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DC.MAS.Remove(ListOfEntries.SelectedItem as MainAnalysisObject);

            MainChart.GetBindingExpression(CartesianChart.SeriesProperty).UpdateTarget();
        }
    }
}
