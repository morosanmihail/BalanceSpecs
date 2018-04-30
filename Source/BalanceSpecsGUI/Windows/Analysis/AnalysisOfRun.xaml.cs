using CsvHelper;
using GeneticAlgorithm.AnalysisTools;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

                var ChartValues = MainAnalysisObject.GetAnalysisTool(SelectedAnalysisTool).GetSeries(MAS.Where(m => m.Enabled).ToList());

                foreach(var Chart in ChartValues)
                {
                    A.Add(new LineSeries { Title = Chart.Item1.Item1, Values = Chart.Item2, ScalesYAt = Chart.Item1.Item2 });
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

        public AnalysisOfRun()
        {
            InitializeComponent();

            this.DataContext = DC;

            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseRightButtonDownEvent, new MouseButtonEventHandler(s_PreviewMouseLeftButtonDown)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(listbox1_Drop)));
            ListOfEntries.ItemContainerStyle = itemContainerStyle;
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
                ResetGraph();
            }

            this.Focus();
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ResetGraph();
        }

        private void ResetGraph()
        {
            MainChart.GetBindingExpression(CartesianChart.SeriesProperty).UpdateTarget();
        }

        void s_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            if (sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        void listbox1_Drop(object sender, DragEventArgs e)
        {
            MainAnalysisObject droppedData = e.Data.GetData(typeof(MainAnalysisObject)) as MainAnalysisObject;
            MainAnalysisObject target = ((ListBoxItem)(sender)).DataContext as MainAnalysisObject;

            int removedIdx = ListOfEntries.Items.IndexOf(droppedData);
            int targetIdx = ListOfEntries.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                DC.MAS.Insert(targetIdx + 1, droppedData);
                DC.MAS.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (DC.MAS.Count + 1 > remIdx)
                {
                    DC.MAS.Insert(targetIdx, droppedData);
                    DC.MAS.RemoveAt(remIdx);
                }
            }

            ResetGraph();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose where to save";
            dlg.IsFolderPicker = false;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;
            dlg.Filters.Add(new CommonFileDialogFilter("Comma Separated Values", "csv"));

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var textWriter = new StreamWriter(dlg.FileName);
                var csv = new CsvWriter(textWriter);
                //var generatedMap = csv.Configuration.AutoMap<ObservablePoint>();
                csv.WriteHeader<ObservablePoint>();

                foreach(var S in MainChart.Series)
                {
                    foreach(ObservablePoint P in S.Values)
                    {
                        csv.WriteRecord(P);
                    }
                }

                textWriter.Close();
            }
        }
    }
}
