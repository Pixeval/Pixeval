using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Objects;
using Pixeval.Persisting;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for PixevalSettingPage.xaml
    /// </summary>
    public partial class PixevalSettingPage : UserControl
    {
        public PixevalSettingPage()
        {
            InitializeComponent();
        }

        public void Open()
        {
            SettingDialog.OpenControl();
        }

        private void ClearCacheButton_OnClick(object sender, RoutedEventArgs e)
        {
            Directory.Delete(PixevalEnvironment.TempFolder, true);
            MainWindow.MessageQueue.Enqueue("清理完成");
        }

        private void SettingDialog_OnDialogClosing(object sender, DialogClosingEventArgs e)
        {
            MainWindow.Instance.SettingsTab.IsSelected = false;
        }

        private void OpenFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog("选择存储位置")
            {
                InitialDirectory = Settings.Global.DownloadLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                IsFolderPicker = true
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok) DownloadLocationTextBox.Text = fileDialog.FileName;
        }

        private void ClearSettingButton_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Global.Initialize();
        }

        private void QueryR18_OnChecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>();
            if (Settings.Global.ExceptTags != null) set.AddRange(Settings.Global.ExceptTags);
            set.AddRange(new[] {"R-18", "R-18G"});
            Settings.Global.ExceptTags = set;
        }

        private void QueryR18_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>(Settings.Global.ExceptTags);
            set.Remove("R-18");
            set.Remove("R-18G");
            Settings.Global.ExceptTags = set;
        }

        private void ExceptTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = ExceptTagTextBox.Text.Split(" ");
            Settings.Global.ExceptTags = new HashSet<string>(text);
        }

        private void ContainsTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = ContainsTagTextBox.Text.Split(" ");
            Settings.Global.ContainsTags = new HashSet<string>(text);
        }
    }
}