using GeneticAlgorithm.GeneticAlgorithm;
using Newtonsoft.Json;
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
            bool DoRun = true;
            do
            {
                try
                {
                    GPRunManager<BalanceGA, List<double>, List<double>> RunManager = null;

                    List<GenericTest<List<double>, List<double>>> tests = new List<GenericTest<List<double>, List<double>>>();

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

                    Console.WriteLine("Successfully loaded batch file");

                    for (int i = 0; i < classNodes.Count; i++)
                    {
                        var ParamFile = classNodes[i].Attributes["paramfile"].Value;
                        var OutputFolder = classNodes[i].Attributes["outputfolder"].Value;
                        var RunCount = int.Parse(classNodes[i].Attributes["runs"].Value);
                        var GenToRun = int.Parse(classNodes[i].Attributes["generations"].Value);
                        var RandomSeed = int.Parse(classNodes[i].Attributes["randomseed"].Value);
                        var EvaluationsMax = int.Parse(classNodes[i].Attributes["evaluations"].Value);

                        var ExperimentName = Path.GetFileName(OutputFolder);

                        for (int Run = 1; Run <= RunCount; Run++)
                        {
                            DoRun = true;
                            var Folder = Path.Combine(OutputFolder, "Run" + Run);
                            var GensToRun = GenToRun;

                            if (Directory.Exists(Folder) && Directory.EnumerateFiles(Folder).Count() > 0)
                            {
                                var rawentries = Directory.GetFiles(Folder, "*.xml");

                                if (rawentries.Count() < GenToRun)
                                {
                                    var entries = rawentries.OrderBy(t => t);

                                    var BackupFilename = entries.Last();

                                    RunManager = GAController.GAController.LoadRunFromFile(BackupFilename);

                                    //string Json = File.ReadAllText(BackupFilename + ".json");
                                    //RunManager = JsonConvert.DeserializeObject<GPRunManager<BalanceGA, int, double>>(Json);
                                    //RunManager = new GPRunManager<BalanceGA, int, double>(ParamFile, tests, RandomSeed * Run);
                                    //JsonConvert.PopulateObject(Json, RunManager);

                                    //RunManager.ReloadParameters();

                                    GensToRun = GenToRun - RunManager.CurrentGen; //entries.Count();

                                    if(RunManager.Populations[0].RunMetrics.TotalFitnessCalculations.Last().Value > EvaluationsMax)
                                    {
                                        DoRun = false;
                                    }
                                }
                                else
                                {
                                    DoRun = false;
                                }
                            }
                            else
                            {
                                var Params = GPRunManager<BalanceGA, List<double>, List<double>>.LoadParamsFromFile(ParamFile);

                                //Change params as needed
                                for(int y = 0; y < classNodes[i].Attributes.Count;y++)
                                {
                                    var Attr = classNodes[i].Attributes[y];
                                    if(Attr.Name.Contains("prop."))
                                    {
                                        var JsonPropName = Attr.Name.Split('.')[1];
                                        Params.JsonParams.gaparams[JsonPropName] = double.Parse(Attr.Value);
                                    }
                                }

                                Params.JsonParameters = JsonConvert.SerializeObject(Params.JsonParams, Newtonsoft.Json.Formatting.Indented);

                                RunManager = new GPRunManager<BalanceGA, List<double>, List<double>>(Params, tests, RandomSeed + Run);

                                RunManager.InitRun();
                            }

                            if (DoRun)
                            {
                                int res = 0;

                                for (int Gener = 0; Gener < GensToRun; Gener++)
                                {
                                    res = RunManager.StartRun(1);
                                    
                                    foreach (BalanceGA FN in RunManager.GetBestMembers())
                                    {
                                        Console.WriteLine(ExperimentName + 
                                            " (" + Run + "." + RunManager.CurrentGen + 
                                            ") - (Fit: " + FN.Fitness + 
                                            ") (Evals: " + RunManager.Populations[0].RunMetrics.TotalFitnessCalculations.Last().Value +
                                            ") (Pred: " + RunManager.Populations[0].Predictor.AcceptedPredictions +
                                            ") (False: " + RunManager.Populations[0].Predictor.FalseNegativesByGeneration.Sum() +
                                            ") (PredAcc: " + RunManager.Populations[0].Predictor.NetworkAccuracy +
                                            ")");
                                        
                                    }

                                    Directory.CreateDirectory(Folder);

                                    string filename = Path.Combine(Folder, "Backup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + "_" + RunManager.CurrentGen + ".xml");

                                    GAController.GAController.SaveRunGAToFile(RunManager, filename);

                                    //JSON
                                    /*JsonSerializer jserializer = new JsonSerializer();
                                    jserializer.NullValueHandling = NullValueHandling.Ignore;
                                    
                                    using(StreamWriter sw = new StreamWriter(filename + ".json"))
                                    using(JsonWriter writer = new JsonTextWriter(sw))
                                    {
                                        jserializer.Serialize(writer, RunManager);
                                    }*/

                                    //Console.WriteLine("Saving to file done");

                                    //Remove other files
                                    var Files = Directory.GetFiles(Folder);
                                    var OrderedFiles = Files.OrderBy(t => t).ToList();
                                    for(int f=0;f<OrderedFiles.Count() - 1;f++)
                                    {
                                        File.Delete(OrderedFiles[f]);
                                    }

                                    if (RunManager.Populations[0].RunMetrics.TotalFitnessCalculations.Last().Value > EvaluationsMax)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error happened: " + e.Message);
                }
            } while (DoRun);
        }
    }
}
