using GeneticAlgorithm.GeneticAlgorithm;
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
        dynamic JSONParams;

        public GAController(string JSONFile)
        {
            KillRun();
            this.JSONFile = JSONFile;
            RunManager = null;
            GenerationsToRun = 1;
            AutosaveLocation = "";
            HeatMapParameterIndexX = 0;
            HeatMapParameterIndexY = 1;

            JSONParams = JsonConvert.DeserializeObject(JSONFile);
        }

        public List<DataPoint> ParetoFront
        {
            get
            {
                if (RunManager != null && RunManager.Populations.Count > 0)
                {
                    var X = new List<DataPoint>();
                    for (int i = 0; i < RunManager.Populations[0].RunMetrics.BestFitnesses.Count; i++)
                    {
                        X.Add(new DataPoint(RunManager.Populations[0].RunMetrics.TotalFitnessCalculations[i].Value, RunManager.Populations[0].RunMetrics.BestFitnesses[i].Value));
                    }
                    return X;
                }
                else
                {
                    return null;
                }
            }

            set { }
        }

        public ChartValues<ObservablePoint> ParetoFront2
        {
            get
            {
                if (RunManager != null && RunManager.Populations.Count > 0)
                {
                    var X = new ChartValues<ObservablePoint>();
                    for (int i = 0; i < RunManager.Populations[0].RunMetrics.BestFitnesses.Count; i++)
                    {
                        X.Add(new ObservablePoint(RunManager.Populations[0].RunMetrics.TotalFitnessCalculations[i].Value, RunManager.Populations[0].RunMetrics.BestFitnesses[i].Value));
                    }
                    return X;
                }
                else
                {
                    return null;
                }
            }
        }

        public ChartValues<double> BestFitnesses2
        {
            get
            {
                if (RunManager != null && RunManager.Populations.Count > 0)
                {
                    var X = new ChartValues<double>();
                    for (int i = 0; i < RunManager.Populations[0].RunMetrics.BestFitnesses.Count; i++)
                    {
                        X.Add(RunManager.Populations[0].RunMetrics.BestFitnesses[i].Value);
                    }
                    return X;
                }
                else
                {
                    return null;
                }
            }
        }

        public ChartValues<double> AverageFitnesses2
        {
            get
            {
                if (RunManager != null && RunManager.Populations.Count > 0)
                {
                    var X = new ChartValues<double>();
                    for (int i = 0; i < RunManager.Populations[0].RunMetrics.AverageFitnesses.Count; i++)
                    {
                        X.Add(RunManager.Populations[0].RunMetrics.AverageFitnesses[i].Value);
                    }
                    return X;
                }
                else
                {
                    return null;
                }
            }
        }

        public int HeatMapParameterIndexX { get; set; }
        public int HeatMapParameterIndexY { get; set; }

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

        public string HeatMapParameterX
        {
            get
            {
                return Parameters[HeatMapParameterIndexX].Name;
            }
        }

        public string HeatMapParameterY
        {
            get
            {
                return Parameters[HeatMapParameterIndexY].Name;
            }
        }

        public ChartValues<HeatPoint> PredictionHeatMap
        {
            get
            {
                if (RunManager != null && RunManager.Populations.Count > 0 && RunManager.Populations[0].UsePredictor)
                {
                    var X = new ChartValues<HeatPoint>();

                    dynamic Params = JSONParams.parameters;

                    double XMin = 0, XMax = 0;
                    double YMin = 0, YMax = 0;

                    int paramsUsed = 0;
                    int totalParams = 0;
                    foreach (var P in Params)
                    {
                        if ((bool)P.enabled == true)
                        {
                            int ListSize = P.listsize != null ? (int)P.listsize : 1;

                            if (ListSize == 1)
                            {
                                if (totalParams == HeatMapParameterIndexX)
                                {
                                    XMin = (double)P.rangeMin;
                                    XMax = (double)P.rangeMax;
                                }
                                if (totalParams == HeatMapParameterIndexY)
                                {
                                    YMin = (double)P.rangeMin;
                                    YMax = (double)P.rangeMax;
                                }

                                paramsUsed++;
                            }
                            totalParams++;
                        }
                    }


                    if (HeatMapParameterIndexX == HeatMapParameterIndexY || HeatMapParameterIndexY >= totalParams || HeatMapParameterIndexX >= totalParams)
                    {
                        return null;
                    }

                    for (double i = XMin; i < XMax; i += (XMax - XMin) / 10)
                    {
                        for (double y = YMin; y < YMax; y += (YMax - YMin) / 10)
                        {
                            var Input = new List<double>();

                            for (int w = 0; w < Math.Min(HeatMapParameterIndexX, HeatMapParameterIndexY); w++)
                            {
                                Input.Add(0);
                            }

                            if (HeatMapParameterIndexY < HeatMapParameterIndexX)
                                Input.Add(y);
                            else
                                Input.Add(i);

                            for (int w = 0; w < Math.Abs(HeatMapParameterIndexY - HeatMapParameterIndexX) - 1; w++)
                            {
                                Input.Add(0);
                            }

                            if (HeatMapParameterIndexY < HeatMapParameterIndexX)
                                Input.Add(i);
                            else
                                Input.Add(y);

                            for (int w = Math.Max(HeatMapParameterIndexY, HeatMapParameterIndexX); w < totalParams - 1; w++)
                            {
                                Input.Add(0);
                            }

                            var Prediction = RunManager.Populations[0].Predictor.Predict(Input);

                            X.Add(new HeatPoint(i, y, Prediction.Sum()));
                        }
                    }

                    if (paramsUsed < 2)
                    {
                        return null;
                    }

                    return X;
                }
                else
                {
                    return null;
                }
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

        public void LoadRunFromFile(string Filename)
        {
            FileStream fs = new FileStream(Filename, FileMode.Open);

            XmlDictionaryReader reader =
                XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(GPRunManager<BalanceGA, List<double>, List<double>>));

            // Deserialize the data and read it from the instance.
            RunManager = (GPRunManager<BalanceGA, List<double>, List<double>>)ser.ReadObject(reader, true);

            ParetoFront = new List<DataPoint>();

            reader.Close();
            fs.Close();

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

                List<GenericTest<List<double>, List<double>>> tests = new List<GenericTest<List<double>, List<double>>>();
                RunManager = new GPRunManager<BalanceGA, List<double>, List<double>>(TempPath, tests);

                GensToRun = GenerationsToRun;

                RunManager.InitRun();
            }

            ParetoFront = new List<DataPoint>();

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
                            try
                            {
                                Directory.CreateDirectory(AutosaveLocation);

                                string filename = Path.Combine(AutosaveLocation, "Backup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ".xml");

                                SaveRunGAToFile(filename);
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
