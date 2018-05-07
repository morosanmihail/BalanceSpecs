using CsvHelper;
using GeneticAlgorithm.AnalysisTools;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class ComparisonResult
    {
        public string FirstFolder { get; set; }
        public string SecondFolder { get; set; }
        public double QuickGood { get { return RatioBetter.Sum() / 4; } }
        public double QuickBad { get { return RatioWorse.Sum() / 4; } }
        public string Result { get { return this.ToString(); } }

        public ComparisonResult(string Folder1, string Folder2, string NameF1, string NameF2)
        {
            FirstFolder = NameF1;
            SecondFolder = NameF2;

            MainAnalysisObject ma1 = new MainAnalysisObject();
            ma1.Initialise(Folder1);

            MainAnalysisObject ma2 = new MainAnalysisObject();
            ma2.Initialise(Folder2);

            var MAOList = new List<MainAnalysisObject>() { ma1, ma2 };

            var Res = StatisticalPAnalysis.PValues(MAOList);
            var Res2 = StatisticalPAnalysis.PValues(MAOList, Accord.Statistics.Testing.TwoSampleHypothesis.FirstValueIsSmallerThanSecond);

            double Threshold = 0.05;

            for (int i = 0; i < 2000; i += 500)
            {
                int RatioGood = 0, RatioBad = 0, Total = 0;
                foreach (var Val in Res.Where(x => x.X < i+500 && x.X >= i))
                {
                    RatioGood += Val.Y < Threshold ? 1 : 0;
                    Total++;
                }

                foreach (var Val in Res2.Where(x => x.X < i + 500 && x.X >= i))
                {
                    RatioBad += Val.Y < Threshold ? 1 : 0;
                    Total++;
                }

                RatioBetter.Add((double)RatioGood / (double)Total);
                RatioWorse.Add((double)RatioBad / (double)Total);
            }
        }

        public List<double> RatioBetter = new List<double>();
        public List<double> RatioWorse = new List<double>();

        public string CompareQuarter(int i)
        {
            if(RatioBetter[i] == 0 && RatioWorse[i] == 0)
            {
                return "Equal in quarter " + i + ". ";
            }

            if(RatioBetter[i] > 0 && RatioWorse[i] == 0)
            {
                return "Better " + (int)(RatioBetter[i] * 100) + "% of the time in quarter " + i + ". ";
            }

            if(RatioBetter[i] == 0 && RatioWorse[i] > 0)
            {
                return "Worse " + (int)(RatioWorse[i] * 100) + "% of the time in quarter " + i + ". ";
            }

            return "Wild results in quarter " + i + ". ";
        }

        public override string ToString()
        {
            string Res = "";

            for(int i = 0; i<RatioBetter.Count;i++)
            {
                Res += CompareQuarter(i);
            }

            return Res;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class RunComparisonModelView
    {
        public ObservableCollection<ComparisonResult> Results { get; set; }

        public string CurrentlyComparing { get; set; }

        public RunComparisonModelView()
        {
            Results = new ObservableCollection<ComparisonResult>();
            CurrentlyComparing = "Comparions";
        }

    }

    /// <summary>
    /// Interaction logic for RunComparison.xaml
    /// </summary>
    public partial class RunComparison : Window
    {
        RunComparisonModelView Model = new RunComparisonModelView();

        public RunComparison()
        {
            InitializeComponent();
            
            this.DataContext = Model;

            Thread test = new Thread(new ThreadStart(DoComparisons));
            test.Start();
        }

        public void DoComparisons()
        {
            //List of folders to compare between
            Dictionary<string, string> Folders = new Dictionary<string, string>();
            string C45Stuff = "F:/ExperimentResults/Thesis/Chapter4/Torcs/C45";
            string KNNStuff = "F:/ExperimentResults/Thesis/Chapter4/Torcs/KNN";
            string NNStuff = "F:/ExperimentResults/Thesis/Chapter4/Torcs/NeuralNetwork";

            Folders.Add("DefaultOptimal", "F:/ExperimentResults/Thesis/Chapter4/Torcs/Torcs-NoPredictor");
            Folders.Add("DefaultUnoptimal", "F:/ExperimentResults/Thesis/Chapter4/Torcs/Torcs-NoPredictor-Mut50-Reinit20");
            //Folders.Add("KNNBase", "F:/ExperimentResults/Thesis/Chapter4/Torcs/Torcs-Mut50-Reinit20-KNN3-T100-200-Acc75");
            //Folders.Add("C45Base", "F:/ExperimentResults/Thesis/Chapter4/Torcs/Torcs-Mut50-Reinit20-C45-T200-200-Acc75");


            foreach (var Folder in Directory.EnumerateDirectories(C45Stuff))
            {
                Folders.Add("C45" + new DirectoryInfo(Folder).Name, Folder);
            }
            foreach (var Folder in Directory.EnumerateDirectories(KNNStuff))
            {
                Folders.Add("KNN" + new DirectoryInfo(Folder).Name, Folder);
            }
            foreach (var Folder in Directory.EnumerateDirectories(NNStuff))
            {
                Folders.Add("NN" + new DirectoryInfo(Folder).Name, Folder);
            }

            //Do comparison here

            //var Comparison = new ComparisonResult(Folders["C45Acc60-First"], Folders["C45Acc85-Third"]);
            //var Comparison2 = new ComparisonResult(Folders["C45Acc85-Third"], Folders["C45Acc60-First"]);

            int TotalComparisons = Folders.Count * (Folders.Count - 1) / 2;
            int Current = 0;

            for(int i=0;i<Folders.Keys.Count;i++)
            //foreach (var Folder in Folders)
            {
                string Folder = Folders.Keys.ElementAt(i);
                for(int y=i+1;y<Folders.Keys.Count;y++)
                //foreach (var Folder2 in Folders)
                {
                    string Folder2 = Folders.Keys.ElementAt(y);
                    Model.CurrentlyComparing = Folder + " to " + Folder2 + "  (" + Current + "/" + TotalComparisons + ")";

                    if (!Folder.Equals(Folder2))
                    {
                        var Comparison = new ComparisonResult(Folders[Folder], Folders[Folder2], Folder, Folder2);
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            Model.Results.Add(Comparison);
                        });
                    }

                    Current++;
                }
            }

            Model.CurrentlyComparing = "Run Comparisons";

            //Save everything to a file
            string FileDestination = "F:/ExperimentResults/Thesis/Chapter4/Torcs/RunComparisons.csv";

            var textWriter = new StreamWriter(FileDestination);
            var csv = new CsvWriter(textWriter);
            //var generatedMap = csv.Configuration.AutoMap<ObservablePoint>();
            csv.WriteHeader<ComparisonResult>();

            foreach (var S in Model.Results)
            {
                csv.WriteRecord(S);
            }

            textWriter.Close();
        }
    }
}
