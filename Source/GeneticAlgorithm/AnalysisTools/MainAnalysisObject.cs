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

        public ChartValues<ObservablePoint> ParetoFront2
        {
            get
            {
                if (GACs != null && GACs.Count > 0)
                {
                    var res = new ChartValues<ObservablePoint>();

                    List<double> AvgFitness = new List<double>();
                    List<double> Evals = new List<double>();

                    int Count = GACs[0].ParetoFront2.Count;

                    foreach (var G in GACs)
                    {
                        if(G.ParetoFront2.Count < Count)
                        {
                            Count = G.ParetoFront2.Count;
                        }
                    }

                    for (int i=0;i<Count;i++)
                    {
                        res.Add(new ObservablePoint(0, 0));
                    }

                    foreach(var G in GACs)
                    {
                        var PF = G.ParetoFront2;
                        for(int i=0;i<Count;i++)
                        {
                            res[i].X += PF[i].X;
                            res[i].Y += PF[i].Y;
                        }
                    }

                    for(int i=0;i<Count;i++)
                    {
                        res[i].X /= GACs.Count;
                        res[i].Y /= GACs.Count;
                    }

                    return res;
                }
                else
                {
                    return null;
                }
            }
        }

        public string ParetoFrontJSON
        {
            get
            {
                var PF = this.ParetoFront2;

                string json = JsonConvert.SerializeObject(PF, Formatting.Indented);

                return json;
            }
        }
    }
}
