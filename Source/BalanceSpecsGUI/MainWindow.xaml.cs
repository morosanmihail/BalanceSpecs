using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls.Dialogs;
using BalanceSpecsGUI.Converters;
using GeneticAlgorithm.GAController;
using BalanceSpecsGUI.Windows;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SharpGenetics.Predictor;

namespace BalanceSpecsGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static dynamic JSONFile = null;

        public MainWindow()
        {
            InitializeComponent();

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            MessageBox.Show(JsonO.gaparams.ToString());
        }

        private async void AddParameterButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            string Name = NewParameterName.Text.Trim();

            if(Name.Length < 1)
            {
                await this.ShowMessageAsync("Error", "Empty names are not allowed!");
                return;
            }

            JsonO.parameters.Add(new JObject(new JProperty("name",Name), new JProperty("enabled",true), new JProperty("listsize",1), new JProperty("minimise","ignore"), new JProperty("custom",new JObject())));
        }

        private async void AddMetricButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;
            
            if(NewMetricName.Text.Trim().Length < 1)
            {
                await this.ShowMessageAsync("Error", "Empty names are not allowed!");
                return;
            }

            try
            {
                JsonO.metrics.Add(new JObject(new JProperty("name", NewMetricName.Text), new JProperty("type", "Double")));
            } catch(Exception ex)
            {
                await this.ShowMessageAsync("Error", "A metric with the same name already exists!");
            }
        }

        private async void AddEvaluatorButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            if (NewEvaluatorName.Text.Trim().Length < 1)
            {
                await this.ShowMessageAsync("Error", "Empty names are not allowed!");
                return;
            }

            try
            {
                JsonO.evaluators.Add(new JObject(new JProperty("name", NewEvaluatorName.Text), new JProperty("enabled",true), new JProperty("type", ReturnEvaluatorTypes.GetTypes()[0])));
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", "An evaluator with the same name already exists!");
            }
        }

        private void AddCustomButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = this.DataContext;

            JsonO.custom[NewCustomData.Text] = "";
        }

        private void AddParamCustomButtonClick(object sender, RoutedEventArgs e)
        {
            dynamic JsonO = (sender as FrameworkElement).DataContext;
            if (JsonO != null)
            {
                JsonO[NewParamCustomData.Text] = "";
            }
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
                GAController.LoadRunFromFile(dlg.FileName);
                
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
    }
}
