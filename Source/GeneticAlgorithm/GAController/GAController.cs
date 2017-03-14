using GeneticAlgorithm.GeneticAlgorithm;
using GeneticAlgorithm.Helpers;
using OxyPlot;
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

namespace GeneticAlgorithm.GAController
{
    [ImplementPropertyChanged]
    public class GAController
    {
        public bool isStarted { get; set; }

        object ThreadLock = new object();

        Thread GAThread = null;

        public GPRunManager<BalanceGA, int, double> RunManager { get; set; }

        /*public ObservableCollection<DataPoint> BestFitnessOverTime
        {
            get
            {
                if(RunManager != null && RunManager.Populations.Count > 0 && RunManager.Populations[0].RunMetrics.BestFitnesses != null)
                    return new AsyncObservableCollection<DataPoint>(RunManager.Populations[0].RunMetrics.BestFitnesses.Select((value,index) => new DataPoint(index,value)));
                return null;
            }
        }*/

        string ResultsFolder = "";
        int GensToRun = 0;

        string Folder;
        int GenToRun;
        int RandomSeed;
        string JSONFile;

        public GAController(string Folder, int GenToRun, int RandomSeed, string JSONFile)
        {
            isStarted = false;
            //BestFitnessOverTime = new AsyncObservableCollection<double>();
            this.Folder = Folder;
            this.GenToRun = GenToRun;
            this.RandomSeed = RandomSeed;
            this.JSONFile = JSONFile;
            RunManager = null;
        }

        public bool StartOrPauseRun()
        {
            lock (ThreadLock)
            {
                isStarted = !isStarted;
            }

            //Do stuff?
            if (isStarted)
            {
                //check if thread exists
                if (GAThread != null && !GAThread.IsAlive)
                {
                    GAThread = null;
                }

                //if not, create it
                //if yes, it will do its thing
                if (GAThread == null)
                {
                    GAThread = new Thread(new ThreadStart(ThreadCode));
                    StartThread();
                }
                else
                {
                    GAThread.Resume();
                }
            }
            else
            {
                GAThread.Suspend();
            }

            return isStarted;
        }

        public void KillRun()
        {
            //kill thread

            if (GAThread != null)
            {
                GAThread.Abort();
            }
            isStarted = false;
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
                bool Run = false;

                lock (ThreadLock)
                {
                    if (isStarted && RunManager != null)
                    {
                        Run = true;
                    }
                }

                if (Run)
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

                }
                Gener++;
            } while (Gener < GensToRun);

            lock (ThreadLock)
            {
                isStarted = false;
            }
        }

    }
}
