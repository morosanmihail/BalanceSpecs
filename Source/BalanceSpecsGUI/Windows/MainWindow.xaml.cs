using BalanceSpecsGUI.Converters;
using BalanceSpecsGUI.Tools;
using GeneticAlgorithm.AnalysisTools;
using GeneticAlgorithm.GAController;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpGenetics.Predictor;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace BalanceSpecsGUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static dynamic JSONFile = null;

        public Analysis.AnalysisOfRun AnalysisWindow = null;

        public MainWindow()
        {
            InitializeComponent();

            ExternalTool.Initialise("BalanceSpecsGUI.Resources.DefaultTools.json");

            NewButton_Click(null, null);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();

            Environment.Exit(0);
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BalanceSpecsGUI.Resources.EmptyBFF.json";

            string result = "";

            //var auxList = System.Reflection.Assembly.GetExecutingAssembly().GetManifes‌​tResourceNames();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            JObject JsonO = JObject.Parse(result);

            this.DataContext = JsonO;
            JSONFile = JsonO;
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            GAController GAController = new GAController(JSONFile.ToString());

            GARun GARunWindow = new GARun(GAController);
            GARunWindow.Show();
        }

        private void MenuItemProgress_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose XML file";
            dlg.IsFolderPicker = false;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;
            dlg.Filters.Add(new CommonFileDialogFilter("Balance File Format", "xml"));

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GAController GAController = new GAController("");
                GAController.LoadRunFromFileAndSetDefaults(dlg.FileName);
                
                GARun GARunWindow = new GARun(GAController);
                GARunWindow.Show();
            }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "BFF JSON|*.json";
            saveFileDialog1.Title = "Save the Balance File Format";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog1.FileName != "")
            {
                var JsonO = this.DataContext as JObject;

                File.WriteAllText(saveFileDialog1.FileName, JsonConvert.SerializeObject(JsonO));
            }
        }

        private void ToolsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pProcess = ExternalTool.RunTool(((sender as System.Windows.Controls.MenuItem).DataContext as ExternalTool).PathToExe);

                if (pProcess != null)
                {
                    pProcess.Start();

                    pProcess.WaitForExit();
                }
            } catch(Exception ex)
            {
                this.ShowMessageAsync("Error", ex.Message);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose BFF file";
            dlg.IsFolderPicker = false;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;
            dlg.Filters.Add(new CommonFileDialogFilter("Balance File Format", "bff,json"));

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var JsonO = JObject.Parse(File.ReadAllText(dlg.FileName));

                this.DataContext = JsonO;
                JSONFile = JsonO;
                if (JSONFile.gaparams.extra_use_predictor != null && JSONFile.gaparams.extra_use_predictor.Value == true)
                {
                    PredictorHelper.ApplyPredictorPropertiesToJsonDynamicAndReturnObjects(JSONFile, (string)JSONFile.gaparams.string_PredictorType);
                }
            }

        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            this.ShowMessageAsync("Error", "Not implemented yet");
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            var ResultsWindow = new SingleRunResults(this.DataContext, 42);

            ResultsWindow.Show();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            var SettingsWindow = new SettingsWindow();

            SettingsWindow.Show();
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            var AddToToolsWindow = new AddToolForm();

            AddToToolsWindow.ShowDialog();
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            var AnalysisWindow = new Analysis.AnalysisOfRun();

            AnalysisWindow.Show();
        }
    }
}
