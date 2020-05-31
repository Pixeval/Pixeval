#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Caching;
using Pixeval.Objects.Generic;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for PixevalSettingPage.xaml
    /// </summary>
    public partial class PixevalSettingPage
    {
        public PixevalSettingPage()
        {
            InitializeComponent();
        }

        private void OpenFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog(AkaI18N.PleaseSelectLocation)
            {
                InitialDirectory = Settings.Global.DownloadLocation ??
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                IsFolderPicker = true
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok) DownloadLocationTextBox.Text = fileDialog.FileName;
        }

        private void QueryR18_OnChecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>();
            if (Settings.Global.ExcludeTag != null) set.AddRange(Settings.Global.ExcludeTag);
            set.AddRange(new[] {"R-18", "R-18G"});
            Settings.Global.ExcludeTag = set;
        }

        private void QueryR18_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>(Settings.Global.ExcludeTag);
            set.Remove("R-18");
            set.Remove("R-18G");
            Settings.Global.ExcludeTag = set;
        }

        private void ExcludeTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = ExcludeTagTextBox.Text.Split(" ");
            Settings.Global.ExcludeTag = new HashSet<string>(text);
        }

        private void IncludeTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = IncludeTagTextBox.Text.Split(" ");
            Settings.Global.IncludeTag = new HashSet<string>(text);
        }

        private void ChangeCachingPolicyToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            AppContext.DefaultCacheProvider.Clear();
            AppContext.DefaultCacheProvider = new FileCache<BitmapImage, Illustration>(AppContext.CacheFolder,
                                                                                       image => image.ToStream(),
                                                                                       InternalIO
                                                                                           .CreateBitmapImageFromStream);
        }

        private void ChangeCachingPolicyToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            AppContext.DefaultCacheProvider.Clear();
            AppContext.DefaultCacheProvider = MemoryCache<BitmapImage, Illustration>.Shared;
        }

        private async void OpenWebApiR18Button_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.MessageQueue.Enqueue(AkaI18N.TryingToToggleR18Switch);
            MainWindow.MessageQueue.Enqueue(await PixivClient.Instance.ToggleWebApiR18State(true)
                                                ? AkaI18N.ToggleR18OnSuccess
                                                : AkaI18N.ToggleR18OnFailed);
        }
    }
}