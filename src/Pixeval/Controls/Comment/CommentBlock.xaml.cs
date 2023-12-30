#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentBlock.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using WinUI3Utilities.Attributes;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls;

[DependencyProperty<CommentBlockViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class CommentBlock
{
    public CommentBlock()
    {
        InitializeComponent();
    }

    public event Action<CommentBlockViewModel>? RepliesHyperlinkButtonTapped;

    public event Action<CommentBlockViewModel>? DeleteHyperlinkButtonTapped;

    private static async void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CommentBlock { ViewModel: { } viewModel } block)
            return;
        if (viewModel.HasReplies)
            _ = viewModel.LoadRepliesAsync();
        _ = viewModel.LoadAvatarSource();
        if (viewModel.IsStamp)
        {
            var result = await App.AppViewModel.MakoClient.DownloadSoftwareBitmapSourceAsync(viewModel.StampSource);
            block.StickerImageContent.Source = result is Result<SoftwareBitmapSource>.Success { Value: var avatar }
                ? avatar
                : await AppContext.GetNotAvailableImageAsync();
        }
        else
        {
            block.CommentContent.Blocks.Add(await viewModel.GetReplyContentParagraphAsync());
        }
    }

    private void PosterPersonPicture_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        // TODO
        // throw new NotImplementedException();
    }

    private void OpenRepliesHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e) => RepliesHyperlinkButtonTapped?.Invoke(ViewModel);

    private void DeleteReplyHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e) => DeleteHyperlinkButtonTapped?.Invoke(ViewModel);
}
