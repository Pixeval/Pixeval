// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.Windows;
using System.Windows.Input;
using Pixeval.Extensions;
using Pixeval.Helpers;
using Pixeval.Models;
using Pixeval.Types;
using Pixeval.Views;
using static Pixeval.Objects.UiHelper;

namespace Pixeval.UserControls
{
    /// <summary>
    ///     Interaction logic for TrendingTagControl.xaml
    /// </summary>
    public partial class TrendingTagControl
    {
        public TrendingTagControl()
        {
            InitializeComponent();
            TrendingTagListBox.ItemsSource = AppContext.TrendingTags;
            SearchingHistoryListBox.ItemsSource = AppContext.GetSearchingHistory();
        }

        private void TrendingTagControl_OnInitialized(object sender, EventArgs e)
        {
            PixivClient.Instance.GetTrendingTags();
        }

        private async void TrendingTagThumbnail_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (await PixivIoHelper.FromUrl(sender.GetDataContext<TrendingTag>().Thumbnail) is { } image) SetImageSource(sender, image);
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