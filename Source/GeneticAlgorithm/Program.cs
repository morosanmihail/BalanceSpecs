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

namespace GeneticAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO
            // This project is the one with the generic balancing code. It invokes sharpgenetics.
            // It will know how to translate the specification file and invoke the bridges
            bool DoRun = true;
            do
            {
                try
                {
                    GPRunManager<BalanceGA, int, double> RunManager = null;

                    List<GenericTest<int, double>> tests = new List<GenericTest<int, double>>();

                    GenericTest<int, double> test = new GenericTest<int, double>();
                    test.AddInput("table", 1);
                    tests.Add(test);

                    string BatchRun = "../RunParams/BatchRun.xml";

                    if (args.Count() > 0)
                    {
                        BatchRun = args[0];
                    }
                    else
                    {
                        //List all batch files in RunParams
                        var ListOfBatch = Directory.EnumerateFiles("../RunParams/", "Batch*.xml").ToList();
                        for (int i = 0; i < ListOfBatch.Count(); i++)
                        {
                            Console.WriteLine(i + ": " + ListOfBatch[i]);
                        }
                        //Select one
                        int Selection = int.Parse(Console.ReadLine());

                        if (Selection < ListOfBatch.Count)
                            BatchRun = ListOfBatch[Selection];
                    }

                    XmlDocument doc1 = new XmlDocument();
                    doc1.Load(BatchRun);

                    var classNodes = doc1.SelectNodes("/batch/run");

                    for (int i = 0; i < classNodes.Count; i++)
                    {
                        var ParamFile = classNodes[i].Attributes["paramfile"].Value;
                        var OutputFolder = classNodes[i].Attributes["outputfolder"].Value;
                        var RunCount = int.Parse(classNodes[i].Attributes["runs"].Value);
                        var GenToRun = int.Parse(classNodes[i].Attributes["generations"].Value);
                        var RandomSeed = int.Parse(classNodes[i].Attributes["randomseed"].Value);


                        for (int Run = 1; Run <= RunCount; Run++)
                        {
                            DoRun = true;
                            var Folder = "Results/" + OutputFolder + "/Run" + Run;
                            var GensToRun = GenToRun;

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
                                    RunManager =
                                        (GPRunManager<BalanceGA, int, double>)ser.ReadObject(reader, true);

                                    //RunManager.ReloadParameters();

                                    reader.Close();
                                    fs.Close();

                                    GensToRun = GenToRun - entries.Count();
                                }
                                else
                                {
                                    DoRun = false;
                                }
                            }
                            else
                            {
                                RunManager = new GPRunManager<BalanceGA, int, double>(ParamFile, tests, RandomSeed * Run);

                                RunManager.InitRun();
                            }

                            if (DoRun)
                            {
                                //System.Console.WriteLine("How many generations to run?");
                                //GenToRun = int.Parse(Console.ReadLine());

                                int res = 0;

                                for (int Gener = 0; Gener < GensToRun; Gener++)
                                {
                                    res = RunManager.StartRun(1);

                                    int c = 0;
                                    foreach (BalanceGA FN in RunManager.GetBestMembers())
                                    {
                                        Console.WriteLine("Population " + c + " - " + FN + " - " + FN.Fitness);
                                        c++;
                                    }

                                    Directory.CreateDirectory(Folder);

                                    string filename = Folder + "/Backup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ".xml";
                                    //FileStream stream = File.Open(filename, FileMode.Create);

                                    var serializer = new DataContractSerializer(typeof(GPRunManager<BalanceGA, int, double>));

                                    var settings = new XmlWriterSettings { Indent = true };

                                    //serializer.WriteObject(stream, RunManager);
                                    using (var w = XmlWriter.Create(filename, settings))
                                    {
                                        serializer.WriteObject(w, RunManager);
                                    }

                                    Console.WriteLine("Saving to file done");

                                    //stream.Close();
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {

                }
            } while (DoRun);
        }
    }
}
