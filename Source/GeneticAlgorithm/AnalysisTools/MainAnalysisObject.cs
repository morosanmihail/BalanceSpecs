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
    [ImplementPropertyChanged]
    public class MainAnalysisObject
    {
        public ObservableCollection<GAController.GAController> GACs { get; set; }

        public string Folder { get; set; }

        public string SelectedAnalysisTool { get; set; }

        public ChartValues<ObservablePoint> SelectedSeries
        {
            get
            {
                //GET SelectedAnalysisTool
                string AType = "GeneticAlgorithm.AnalysisTools." + SelectedAnalysisTool + ",GeneticAlgorithm";

                var Tool = (AnalysisTool)Activator.CreateInstance(Type.GetType(AType), new object[] { this });
                //AnalysisTool Tool = new ParetoFrontAnalysis(this);

                return Tool.SeriesData;
            }
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

            if(Directory.EnumerateFiles(Folder, "*.xml").Count() > 0)
            {
                //It's a basic folder
                Folders.Add(Folder);
            }

            var dirs = Directory.EnumerateDirectories(Folder);
            if(dirs.Count() > 0 && Directory.EnumerateFiles(dirs.First(), "*.xml").Count() > 0)
            {
                //It's a folder with multiple runs in it
                Folders.AddRange(dirs);
            }

            foreach(var f in Folders)
            {
                var Filename = Directory.EnumerateFiles(f).Last();
                GAController.GAController GAController = new GAController.GAController("");
                GAController.LoadRunFromFileAndSetDefaults(Filename);

                GACs.Add(GAController);
            }
        }
    }
}
