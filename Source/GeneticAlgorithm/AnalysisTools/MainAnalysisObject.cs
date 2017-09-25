using LiveCharts;
using LiveCharts.Defaults;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.AnalysisTools
{
    [AddINotifyPropertyChangedInterface]
    public class MainAnalysisObject
    {
        public ObservableCollection<GAController.GAController> GACs { get; set; }

        public string Folder { get; set; }

        public string SelectedAnalysisTool { get; set; }

        public bool Enabled { get; set; }

        /*public ChartValues<ObservablePoint> SelectedSeries
        {
            get
            {
                return GetSeries(SelectedAnalysisTool);
            }
        }

        public ChartValues<HeatPoint> SelectedHeatMap
        {
            get
            {
                return GetHeatMap(SelectedAnalysisTool, 0, 1);
            }
        }*/

        public MainAnalysisObject()
        {
            Enabled = true;
        }

        public static AnalysisTool GetAnalysisTool(string Tool)
        {
            string AType = "GeneticAlgorithm.AnalysisTools." + Tool + ",GeneticAlgorithm";

            var NTool = (AnalysisTool)Activator.CreateInstance(Type.GetType(AType), new object[] { });

            return NTool;
        }

        public ChartValues<ObservablePoint> GetSeries(string SelectedTool)
        {
            return GetAnalysisTool(SelectedTool).GetSeries(new List<MainAnalysisObject>() { this })[0].Item2;
        }

        public ChartValues<HeatPoint> GetHeatMap(string SelectedTool, int X, int Y)
        {
            /*var Tool = GetAnalysisTool(SelectedTool);
            Tool.X = X;
            Tool.Y = Y;
            return Tool.SeriesHeatMap;*/
            return null;
        }

        public void Initialise(GAController.GAController GAC)
        {
            this.Folder = "";

            GACs = new ObservableCollection<GAController.GAController>();
            GACs.Add(GAC);
        }

        public void Initialise(string Folder)
        {
            this.Folder = Path.GetFileName(Folder);

            GACs = new ObservableCollection<GAController.GAController>();

            List<string> Folders = new List<string>();

            if (Directory.EnumerateFiles(Folder, "*.xml").Count() > 0)
            {
                //It's a basic folder
                Folders.Add(Folder);
            }

            var dirs = Directory.EnumerateDirectories(Folder);
            if (dirs.Count() > 0 && Directory.EnumerateFiles(dirs.First(), "*.xml").Count() > 0)
            {
                //It's a folder with multiple runs in it
                Folders.AddRange(dirs);
            }

            foreach (var f in Folders)
            {
                var Filename = Directory.EnumerateFiles(f).Last();
                GAController.GAController GAController = new GAController.GAController("");
                GAController.LoadRunFromFileAndSetDefaults(Filename);

                GACs.Add(GAController);
            }
        }
    }
}
