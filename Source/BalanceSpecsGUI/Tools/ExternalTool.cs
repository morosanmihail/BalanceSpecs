using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BalanceSpecsGUI.Tools
{
    [AddINotifyPropertyChangedInterface]
    [DataContract]
    public class ExternalTool
    {
        [DataMember]
        public string PathToExe { get; set; }

        [DataMember]
        public string Name { get; set; }

        public ExternalTool()
        {

        }

        public ExternalTool(string Path, string Name)
        {
            this.PathToExe = Path;
            this.Name = Name;
        }

        public static void Initialise(string DefaultFile)
        {
            if (Properties.Settings.Default.Tools2 == null)
            {
                Properties.Settings.Default.Tools2 = new System.Collections.ObjectModel.ObservableCollection<ExternalTool>();
            }

            if (Properties.Settings.Default.FirstRun)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = DefaultFile;

                string result = "";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                dynamic JsonO = JArray.Parse(result);

                foreach(var El in JsonO)
                {
                    AddToolToSettings((string)El.PathToExe, (string)El.Name);
                }

                Properties.Settings.Default.FirstRun = false;
            }
        }

        public static void AddToolToSettings(string Path, string Name)
        {
            var Tool = new ExternalTool(Path, Name);

            Properties.Settings.Default.Tools2.Add(Tool);

            Properties.Settings.Default.Save();
        }

        public static System.Diagnostics.Process RunTool(string PathString)
        {
            //Go through each %X in PathString and do something

            string InvokePath = "";
            bool EscapeInvoke = false;

            for(int i=0;(i<PathString.Length && !EscapeInvoke);i++)
            {
                if(PathString[i] == '%' && i < PathString.Length-1)
                {
                    switch(PathString[i+1])
                    {
                        case 'f':
                            var Filename = LoadFile();

                            if(Filename.Length == 0)
                            {
                                EscapeInvoke = true;
                            }

                            InvokePath += '"' + Filename + '"';
                            break;
                        case 'F':
                            var Folder = LoadFolder();

                            if (Folder.Length == 0)
                            {
                                EscapeInvoke = true;
                            }

                            InvokePath += '"' + Folder + '"';
                            break;
                        default:
                            break;
                    }

                    i++;
                } else
                {
                    InvokePath += PathString[i];
                }
            }

            if(EscapeInvoke)
            {
                return null;
            }

            //Actually run InvokePath TODO
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = InvokePath.Split(' ')[0];
            pProcess.StartInfo.Arguments = string.Join(" ", InvokePath.Split(' ').Skip(1));
            pProcess.StartInfo.UseShellExecute = true;
            pProcess.StartInfo.RedirectStandardOutput = false;
            pProcess.EnableRaisingEvents = true;
            //pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            //pProcess.Start();
            //string output = pProcess.StandardOutput.ReadToEnd(); //The output result
            //pProcess.WaitForExit();

            return pProcess;
        }

        static string LoadFolder()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose folder";
            dlg.IsFolderPicker = true;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsureFileExists = false;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dlg.FileName;
            }

            return "";
        }

        static string LoadFile()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose file";
            dlg.IsFolderPicker = false;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dlg.FileName;
            }

            return "";
        }
    }
}
