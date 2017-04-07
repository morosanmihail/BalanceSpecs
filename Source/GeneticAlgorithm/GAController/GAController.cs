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

        public bool IsAutosaving { get; set; }

        public bool IsNotAutosaving { get { return !IsAutosaving; } }

        public string AutosaveLocation { get; set; }

        public GPRunManager<BalanceGA, List<double>, List<double>> RunManager { get; set; }

        public int GenerationsToRun { get; set; }
        int GensToRun = 0;
        
        string JSONFile;

        public GAController(string JSONFile)
        {
            isStarted = false;
            this.JSONFile = JSONFile;
            RunManager = null;
            GenerationsToRun = 1;
            AutosaveLocation = "";
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

        public void LoadRunFromFile(string Filename)
        {
            FileStream fs = new FileStream(Filename, FileMode.Open);

            XmlDictionaryReader reader =
                XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(GPRunManager<BalanceGA, List<double>, List<double>>));

            // Deserialize the data and read it from the instance.
            RunManager = (GPRunManager<BalanceGA, List<double>, List<double>>)ser.ReadObject(reader, true);

            reader.Close();
            fs.Close();
        }

        private void StartThread()
        {
            //TODO figure out situations where autosave is off and the run resets from the last autosave regardless
            bool FromScratch = true;

            GensToRun = GenerationsToRun;

            if (IsAutosaving && Directory.Exists(AutosaveLocation))
            {
                var rawentries = Directory.GetFiles(AutosaveLocation, "*.xml");

                if (rawentries.Count() > 0)
                {
                    var entries = rawentries.OrderBy(t => t);

                    var BackupFilename = entries.Last();

                    LoadRunFromFile(BackupFilename);

                    if (rawentries.Count() < GenerationsToRun)
                    {
                        GensToRun = GenerationsToRun - entries.Count();
                    }
                    else
                    {
                        //Done!
                        //DoRun = false;

                        GensToRun = 0;
                    }

                    FromScratch = false;
                }
            }

            if (FromScratch && RunManager == null)
            {
                string TempPath = Path.GetTempFileName() + ".json";
                File.WriteAllText(TempPath, JSONFile);

                List<GenericTest<List<double>, List<double>>> tests = new List<GenericTest<List<double>, List<double>>>();
                RunManager = new GPRunManager<BalanceGA, List<double>, List<double>>(TempPath, tests);

                GensToRun = GenerationsToRun;

                RunManager.InitRun();
            }

            GAThread.Start();
        }

        public void SaveRunGAToFile(string Filename)
        {
            var serializer = new DataContractSerializer(typeof(GPRunManager<BalanceGA, List<double>, List<double>>));

            var settings = new XmlWriterSettings { Indent = true };

            //serializer.WriteObject(stream, RunManager);
            using (var w = XmlWriter.Create(Filename, settings))
            {
                serializer.WriteObject(w, RunManager);
            }
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
                        
                        if (IsAutosaving)
                        {
                            Directory.CreateDirectory(AutosaveLocation);

                            string filename = AutosaveLocation + "/Backup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ".xml";

                            SaveRunGAToFile(filename);
                        }
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
