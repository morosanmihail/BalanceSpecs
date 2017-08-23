using GeneticAlgorithm.GAController;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;

namespace BalanceSpecsGUI.Windows
{
    /// <summary>
    /// Interaction logic for GARun.xaml
    /// </summary>
    public partial class GARun : MetroWindow
    {
        public GARun(GAController Controller)
        {
            InitializeComponent();

            this.DataContext = Controller;
        }

        protected override void OnClosed(EventArgs e)
        {
            var GAC = this.DataContext as GAController;

            if (GAC != null)
            {
                GAC.KillRun();
            }

            base.OnClosed(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var GAC = this.DataContext as GAController;
            
            GAC.StartOrPauseRun();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var GAC = this.DataContext as GAController;

            GAC.KillRun();
        }

        private void ManualSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var GAC = this.DataContext as GAController;

            string Filename = "";

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML file (*.xml)|*.xml";
            
            if (saveFileDialog.ShowDialog() == true)
            {
                Filename = saveFileDialog.FileName;

                GAController.SaveRunGAToFile(GAC.RunManager, Filename);
            }
        }

        private void AutosaveLocationTextbox_Click(object sender, RoutedEventArgs e)
        {
            var GAC = this.DataContext as GAController;

            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose Autosave location";
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
                GAC.AutosaveLocation = dlg.FileName;
                GAC.IsAutosaving = true;
                //AutosaveLocationTextbox.Text = dlg.FileName;
            }
        }

        private void MetroWindow_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var GAC = this.DataContext as GAController;

            if (GAC != null && GAC.isStarted)
            {
                MessageBoxResult result = MessageBox.Show("Run is still in progress. Are you sure you want to close?", "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
