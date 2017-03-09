using GeneticAlgorithm.GeneticAlgorithm;
using SharpGenetics.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GeneticAlgorithm.GAController
{
    public class GAController
    {
        bool isStarted = false;

        object ThreadLock = new object();

        GPRunManager<BalanceGA, int, double> RunManager = null;

        string ResultsFolder = "";
        int GensToRun = 0;

        public GAController()
        {

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
                //if not, create it
                //if yes, it will do its thing
            }

            return isStarted;
        }

        public void KillRun()
        {
            //kill thread

            lock(ThreadLock)
            {
                isStarted = false;
            }
        }

        private void StartThread(string Folder, int GenToRun, int RandomSeed, string JSONFile)
        {
            GensToRun = GenToRun;

            if (Directory.Exists(Folder))
            {
                var rawentries = Directory.GetFiles(Folder, "*.xml");

                if (rawentries.Count() < GenToRun)
                {
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

                    GensToRun = GenToRun - entries.Count();
                }
                else
                {
                    //Done!
                    //DoRun = false;
                }
            }
            else
            {
                string TempPath = Path.GetTempFileName();
                File.WriteAllText(TempPath, JSONFile);

                List<GenericTest<int, double>> tests = new List<GenericTest<int, double>>();
                RunManager = new GPRunManager<BalanceGA, int, double>(TempPath, tests, RandomSeed);

                RunManager.InitRun();
            }

            ResultsFolder = Folder;
        }

        private void ThreadCode()
        {
            for (int Gener = 0; Gener < GensToRun; Gener++)
            {
                lock (ThreadLock)
                {
                    if(isStarted)
                    {
                        int res = RunManager.StartRun(1);

                        int c = 0;
                        foreach (BalanceGA FN in RunManager.GetBestMembers())
                        {
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
            }
        }
    }
}
