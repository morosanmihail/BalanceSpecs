﻿using GeneticAlgorithm.GAController;
using MahApps.Metro.Controls;
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
    public partial class GARun
    {
        public GARun()
        {
            InitializeComponent();
            //((INotifyCollectionChanged)ProgressListBox.Items).CollectionChanged += ListView_CollectionChanged;
        }

        /*private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset) && ProgressListBox.Items.Count > 0)
            {
                // scroll the new item into view   
                ProgressListBox.ScrollIntoView(ProgressListBox.Items.GetItemAt(ProgressListBox.Items.Count-1));
            }
        }*/

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

                GAC.SaveRunGAToFile(Filename);
            }
        }

        private void AutosaveLocationTextbox_Click(object sender, RoutedEventArgs e)
        {
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
                AutosaveLocationTextbox.Text = dlg.FileName;
            }
        }
    }
}