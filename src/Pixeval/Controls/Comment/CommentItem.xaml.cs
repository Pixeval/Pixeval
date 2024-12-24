#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentItem.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<CommentItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class CommentItem
{
    public CommentItem() => InitializeComponent();

    public event Action<CommentItemViewModel>? RepliesHyperlinkButtonClick;

    public event Action<CommentItemViewModel>? DeleteHyperlinkButtonClick;

    private static async void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CommentItem { ViewModel: { } viewModel } block)
            return;
        if (viewModel.HasReplies)
            _ = viewModel.LoadRepliesAsync();
        _ = viewModel.LoadAvatarSource();
        if (viewModel.IsStamp)
        {
            block.StickerImageContent.Source = await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>().GetSourceFromMemoryCacheAsync(viewModel.StampSource);
        }
        else
        {
            block.CommentContent.Blocks.Clear();
            block.CommentContent.Blocks.Add(await viewModel.GetReplyContentParagraphAsync());
        }
    }

    private async void PosterPersonPicture_OnClicked(object sender, RoutedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(ViewModel.PosterId);
    }

    private void OpenRepliesHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => RepliesHyperlinkButtonClick?.Invoke(ViewModel);

    private void DeleteReplyHyperlinkButton_OnClicked(object sender, RoutedEventArgs e) => DeleteHyperlinkButtonClick?.Invoke(ViewModel);
}
