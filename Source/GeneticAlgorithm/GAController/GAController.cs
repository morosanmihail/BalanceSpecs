﻿using GeneticAlgorithm.GeneticAlgorithm;
using GeneticAlgorithm.Helpers;
using LiveCharts;
using LiveCharts.Defaults;
using Newtonsoft.Json;
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
    public class IndexName
    {
        public string Name;
        public int Index;

        public IndexName(string N, int I)
        {
            Name = N;
            Index = I;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class GAController
    {
        public bool isStarted { get; set; }

        object ThreadLock = new object();

        Thread GAThread = null;

        public bool IsAutosaving { get; set; }

        public bool IsNotAutosaving { get { return !IsAutosaving; } }

        public string AutosaveLocation { get; set; }

        public GPRunManager<BalanceGA> RunManager { get; set; }

        public int GenerationsToRun { get; set; }
        int GensToRun = 0;

        string JSONFile;
        public dynamic JSONParams;

        public GAController(string JSONFile)
        {
            KillRun();
            this.JSONFile = JSONFile;
            RunManager = null;
            GenerationsToRun = 1;
            AutosaveLocation = "";

            JSONParams = JsonConvert.DeserializeObject(JSONFile);
        }

        public List<IndexName> Parameters
        {
            get
            {
                List<IndexName> Res = new List<IndexName>();

                dynamic Params = JSONParams.parameters;
                int totalParams = 0;

                foreach (var P in Params)
                {
                    if ((bool)P.enabled == true)
                    {
                        int ListSize = P.listsize != null ? (int)P.listsize : 1;

                        if (ListSize == 1)
                        {
                            Res.Add(new IndexName((string)P.name, totalParams));
                        }

                        totalParams++;
                    }
                }

                return Res;
            }
        }

        public List<string> EnabledParameters
        {
            get
            {
                if (JSONFile != null)
                {
                    var X = new List<string>();
                    foreach (var P in JSONParams.parameters)
                    {
                        if (P.enabled.Value == true)
                        {
                            X.Add((string)P.name);
                        }
                    }
                    return X;
                }
                else
                {
                    return null;
                }
            }
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
            try
            {
                if (GAThread != null)
                {
                    GAThread.Abort();
                }
            }
            catch { }
            finally
            {
                isStarted = false;
                GAThread = null;
            }
        }

        public static void SaveRunGAToFile(GPRunManager<BalanceGA> RunManager, string Filename)
        {
            var serializer = new DataContractSerializer(typeof(GPRunManager<BalanceGA>));

            var settings = new XmlWriterSettings { Indent = true };

            //serializer.WriteObject(stream, RunManager);
            using (var w = XmlWriter.Create(Filename, settings))
            {
                serializer.WriteObject(w, RunManager);
            }
        }

        public static GPRunManager<BalanceGA> LoadRunFromFile(string Filename)
        {
            FileStream fs = new FileStream(Filename, FileMode.Open);

            var quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 2147483647;
            quotas.MaxStringContentLength = 2147483647;

            XmlDictionaryReader reader =
                XmlDictionaryReader.CreateTextReader(fs, quotas);
            DataContractSerializer ser = new DataContractSerializer(typeof(GPRunManager<BalanceGA>));

            // Deserialize the data and read it from the instance.
            var RunManager = (GPRunManager<BalanceGA>)ser.ReadObject(reader, true);
            
            reader.Close();
            fs.Close();

            return RunManager;
        }

        public void LoadRunFromFileAndSetDefaults(string Filename)
        {
            RunManager = LoadRunFromFile(Filename);

            //ParetoFront = new ObservableCollection<DataPoint>();

            JSONParams = RunManager.Parameters.JsonParams;

            IsAutosaving = true;
            AutosaveLocation = Path.GetDirectoryName(Filename);
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
                
                RunManager = new GPRunManager<BalanceGA>(TempPath);

                GensToRun = GenerationsToRun;

                RunManager.InitRun();
            }

            //ParetoFront = new List<DataPoint>();

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

                        if (IsAutosaving)
                        {
                            try
                            {
                                Directory.CreateDirectory(AutosaveLocation);

                                string filename = Path.Combine(AutosaveLocation, "Backup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ".xml");

                                SaveRunGAToFile(RunManager, filename);
                            }
                            catch (Exception e)
                            {

                            }
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
