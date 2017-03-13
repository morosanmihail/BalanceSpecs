using GeneticAlgorithm.GeneticAlgorithm;
using GeneticAlgorithm.Helpers;
using PropertyChanged;
using SharpGenetics.BaseClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ZedGraph;

namespace GeneticAlgorithm.GAController
{
    [ImplementPropertyChanged]
    public class GAController
    {
        public bool isStarted { get; set; }

        object ThreadLock = new object();

        Thread GAThread = null;

        public ZedGraphControl ZGC = null;

        public GPRunManager<BalanceGA, int, double> RunManager = null;
        
        public ObservableCollection<double> BestFitnessOverTime
        {
            get;set;
        }

        string ResultsFolder = "";
        int GensToRun = 0;

        string Folder;
        int GenToRun;
        int RandomSeed;
        string JSONFile;

        public GAController(string Folder, int GenToRun, int RandomSeed, string JSONFile)
        {
            isStarted = false;
            BestFitnessOverTime = new AsyncObservableCollection<double>();
            this.Folder = Folder;
            this.GenToRun = GenToRun;
            this.RandomSeed = RandomSeed;
            this.JSONFile = JSONFile;
        }
        
        public bool StartOrPauseRun()
        {
            lock (ThreadLock)
            {
                isStarted = !isStarted;
            }

            //Do stuff?
            if(isStarted)
            {
                //check if thread exists
                if(GAThread != null && !GAThread.IsAlive)
                {
                    GAThread = null;
                }

                //if not, create it
                //if yes, it will do its thing
                if(GAThread == null)
                {
                    GAThread = new Thread(new ThreadStart(ThreadCode));
                    StartThread();
                } else
                {
                    GAThread.Resume();
                }
            } else
            {
                GAThread.Suspend();
            }

            return isStarted;
        }

        public void KillRun()
        {
            //kill thread

            lock(ThreadLock)
            {
                isStarted = false;
                if(GAThread != null)
                {
                    GAThread.Abort();
                }
            }
        }

        private void StartThread()
        {
            GensToRun = GenToRun;

            if (Directory.Exists(Folder))
            {
                var rawentries = Directory.GetFiles(Folder, "*.xml");

                var entries = rawentries.OrderBy(t => t);

                var BackupFilename = entries.Last();

                FileStream fs = new FileStream(BackupFilename, FileMode.Open);

                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(GPRunManager<BalanceGA, int, double>));

                // Deserialize the data and read it from the instance.
                RunManager = (GPRunManager<BalanceGA, int, double>)ser.ReadObject(reader, true);

                reader.Close();
                fs.Close();

                if (rawentries.Count() < GenToRun)
                {
                    GensToRun = GenToRun - entries.Count();
                }
                else
                {
                    //Done!
                    //DoRun = false;

                    GensToRun = 0;
                }
            }
            else
            {
                string TempPath = Path.GetTempFileName() + ".json";
                File.WriteAllText(TempPath, JSONFile);

                List<GenericTest<int, double>> tests = new List<GenericTest<int, double>>();
                RunManager = new GPRunManager<BalanceGA, int, double>(TempPath, tests, RandomSeed);

                RunManager.InitRun();
            }

            ResultsFolder = Folder;

            GAThread.Start();
        }

        private void ThreadCode()
        {
            int Gener = 0;
            do
            {
                //for (int Gener = 0; Gener < GensToRun; Gener++)
                //{
                lock (ThreadLock)
                {
                    if (isStarted && RunManager != null)
                    {
                        if (GensToRun > 0)
                        {
                            int res = RunManager.StartRun(1);

                            int c = 0;
                            foreach (BalanceGA FN in RunManager.GetBestMembers())
                            {
                                //BestFitnessOverTime.Add(FN.Fitness);

                                Console.WriteLine("Population " + c + " - " + FN + " - " + FN.Fitness);
                                c++;
                            }

                            //if autosave
                            Directory.CreateDirectory(ResultsFolder);

                            string filename = ResultsFolder + "/Backup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ".xml";

                            var serializer = new DataContractSerializer(typeof(GPRunManager<BalanceGA, int, double>));

                            var settings = new XmlWriterSettings { Indent = true };

                            //serializer.WriteObject(stream, RunManager);
                            using (var w = XmlWriter.Create(filename, settings))
                            {
                                serializer.WriteObject(w, RunManager);
                            }

                            Console.WriteLine("Saving to file done");
                        }

                        BestFitnessOverTime = new ObservableCollection<double>(RunManager.Populations[0].RunMetrics.BestFitnesses);

                        GenerateGraph(ZGC);
                    }
                }
                Gener++;
            } while (Gener < GensToRun);

            isStarted = false;
        }

        public void GenerateGraph(ZedGraphControl zgc, bool ShowAverage = true, bool ShowBest = true, bool ShowAverageBest = true, bool ShowComputedReal = false, bool ShowComputedRealAvg = false, bool ShowComputedRealNewBest = false)
        {
            try
            {
                GraphPane myPane = zgc.GraphPane;
                myPane.CurveList.Clear();

                myPane.Border.IsVisible = false;

                // Set the titles
                myPane.Title.IsVisible = false;
                myPane.Chart.Border.IsVisible = false;
                myPane.XAxis.Title.Text = "Generation";
                myPane.YAxis.Title.Text = "Fitness";
                myPane.XAxis.IsAxisSegmentVisible = true;
                myPane.YAxis.IsAxisSegmentVisible = true;
                myPane.XAxis.MinorGrid.IsVisible = false;
                myPane.YAxis.MinorGrid.IsVisible = false;
                myPane.XAxis.MinorTic.IsOpposite = false;
                myPane.XAxis.MajorTic.IsOpposite = false;
                myPane.YAxis.MinorTic.IsOpposite = false;
                myPane.YAxis.MajorTic.IsOpposite = false;
                myPane.XAxis.Scale.MinGrace = 0;
                myPane.XAxis.Scale.MaxGrace = 0;
                myPane.YAxis.Scale.MinGrace = 0;
                myPane.YAxis.Scale.MaxGrace = 0;

                LineItem myCurve = null;

                if (ShowBest)
                {
                    myCurve = myPane.AddCurve("Best Fitness", Enumerable.Range(1, BestFitnessOverTime.Count).Select(i => (double)i).ToArray(), BestFitnessOverTime.ToArray(), Color.Red);
                    myCurve.Line.IsAntiAlias = true;
                    myCurve.Line.IsVisible = true;
                    myCurve.Symbol.IsVisible = false;
                }
                /*
                if (ShowAverage)
                {
                    myCurve = myPane.AddCurve("Average Fitness", Res.Generations.ToArray(), Res.AvgFitness.ToArray(), Color.Blue);
                    myCurve.Line.IsAntiAlias = true;
                    myCurve.Line.IsVisible = true;
                    myCurve.Symbol.IsVisible = false;
                }

                if (ShowAverageBest && Res.AvgBestFitness.Count > 0)
                {
                    myCurve = myPane.AddCurve("Avg Best Fitness", Res.Generations.ToArray(), Res.AvgBestFitness.ToArray(), Color.DarkSalmon);
                    myCurve.Line.IsAntiAlias = true;
                    myCurve.Line.IsVisible = true;
                    myCurve.Symbol.IsVisible = false;
                }

                if (ShowComputedReal)
                {
                    try
                    {
                        List<double> BestReal = new List<double>();
                        //List<double> AvgReal = new List<double>();

                        for (int i = 0; i < Res.Generations.Count; i++)
                        {
                            BestReal.Add(Res.RealFitnessOfBestFive[i][0]);
                            //AvgReal.Add(Res.RealFitnessOfBestFive[i].Average());
                        }

                        myCurve = myPane.AddCurve("Real Fitness", Res.Generations.ToArray(), BestReal.ToArray(), Color.Green);
                        myCurve.Line.IsAntiAlias = true;
                        myCurve.Line.IsVisible = true;
                        myCurve.Symbol.IsVisible = false;
                    }
                    catch { }
                }

                if (ShowComputedRealNewBest)
                {
                    try
                    {
                        List<double> BestReal = new List<double>();
                        //List<double> AvgReal = new List<double>();

                        for (int i = 0; i < Res.Generations.Count; i++)
                        {
                            BestReal.Add(Res.RealFitnessOfBestFive[i].Min());
                            //AvgReal.Add(Res.RealFitnessOfBestFive[i].Average());
                        }

                        myCurve = myPane.AddCurve("True Best Fitness", Res.Generations.ToArray(), BestReal.ToArray(), Color.Black);
                        myCurve.Line.IsAntiAlias = true;
                        myCurve.Line.IsVisible = true;
                        myCurve.Symbol.IsVisible = false;
                    }
                    catch { }
                }

                if (ShowComputedRealAvg)
                {
                    try
                    {
                        List<double> AvgReal = new List<double>();

                        for (int i = 0; i < Res.Generations.Count; i++)
                        {
                            AvgReal.Add(Res.RealFitnessOfBestFive[i].Average());
                        }

                        myCurve = myPane.AddCurve("Real Fitness Top 5 Avg", Res.Generations.ToArray(), AvgReal.ToArray(), Color.MediumPurple);
                        myCurve.Line.IsAntiAlias = true;
                        myCurve.Line.IsVisible = true;
                        myCurve.Symbol.IsVisible = false;
                    }
                    catch { }
                } */

                zgc.AxisChange();
                zgc.Invalidate();
            } catch(Exception e)
            { }
        }
    }
}
