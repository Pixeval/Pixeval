using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Caching.Persisting;

namespace Pixeval
{
    public partial class MainWindow
    {
        private void OpenFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new CommonOpenFileDialog("选择存储位置")
            {
                InitialDirectory = Settings.Global.DownloadLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                IsFolderPicker = true
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DownloadLocationTextBox.Text = fileDialog.FileName;
            }
        }

        private void ClearSettingButton_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Global.Clear();
        }
    }
}