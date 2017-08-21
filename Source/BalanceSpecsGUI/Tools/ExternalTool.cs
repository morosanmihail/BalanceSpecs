using Microsoft.WindowsAPICodePack.Dialogs;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BalanceSpecsGUI.Tools
{
    [ImplementPropertyChanged]
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

        public static void RunTool(string PathString)
        {
            //Go through each %X in PathString and do something

            string InvokePath = "";

            for(int i=0;i<PathString.Length;i++)
            {
                if(PathString[i] == '%' && i < PathString.Length-1)
                {
                    switch(PathString[i+1])
                    {
                        case 'f':
                            InvokePath += '"' + LoadFile() + '"';
                            break;
                        case 'F':
                            InvokePath += '"' + LoadFolder() + '"';
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

            //Actually run InvokePath TODO
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = InvokePath.Split(' ')[0];
            pProcess.StartInfo.Arguments = string.Join(" ", InvokePath.Split(' ').Skip(1));
            pProcess.StartInfo.UseShellExecute = true;
            pProcess.StartInfo.RedirectStandardOutput = false;
            //pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            pProcess.Start();
            //string output = pProcess.StandardOutput.ReadToEnd(); //The output result
            pProcess.WaitForExit();
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
