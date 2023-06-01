﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

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
using System.Windows;
using System.Windows.Input;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Generic;
using static Pixeval.Objects.Primitive.UiHelper;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for TrendingTagControl.xaml
    /// </summary>
    public partial class TrendingTagControl
    {
        public TrendingTagControl()
        {
            InitializeComponent();
            TrendingTagListBox.ItemsSource = PixevalContext.TrendingTags;
            SearchingHistoryListBox.ItemsSource = SearchingHistoryManager.GetSearchingHistory();
        }

        private async void TrendingTagControl_OnInitialized(object sender, EventArgs e)
        {
           PixevalContext.TrendingTags.AddRange(await PixivClient.GetTrendingTags());
        }

        private async void TrendingTagThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (await PixivIO.FromUrl(sender.GetDataContext<TrendingTag>().Thumbnail) is { } image)
            {
                SetImageSource(sender, image);
            }
        }

        private void SearchingHistoryContent_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.KeywordTextBox.Text = sender.GetDataContext<string>();
        }

        private void TrendingTagItemContainer_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.KeywordTextBox.Text = sender.GetDataContext<TrendingTag>().Tag;
        }
    }
}