#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CommentBlock.xaml.cs
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
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.UserControls
{
    public sealed partial class CommentBlock
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CommentBlockViewModel),
            typeof(CommentBlock),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, PropertyChangedCallback));

        private EventHandler<TappedRoutedEventArgs>? _deleteHyperlinkButtonTapped;

        private EventHandler<TappedRoutedEventArgs>? _repliesHyperlinkButtonTapped;

        public CommentBlock()
        {
            InitializeComponent();
        }

        public CommentBlockViewModel ViewModel
        {
            get => (CommentBlockViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public event EventHandler<TappedRoutedEventArgs> RepliesHyperlinkButtonTapped
        {
            add => _repliesHyperlinkButtonTapped += value;
            remove => _repliesHyperlinkButtonTapped -= value;
        }

        public event EventHandler<TappedRoutedEventArgs> DeleteHyperlinkButtonTapped
        {
            add => _deleteHyperlinkButtonTapped += value;
            remove => _deleteHyperlinkButtonTapped -= value;
        }

        private static async void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var block = (CommentBlock) d;
            if (e.NewValue is not CommentBlockViewModel viewModel)
            {
                return;
            }

            if (viewModel.HasReplies)
            {
                _ = viewModel.LoadRepliesAsync().ContinueWith(_ => block.OpenRepliesHyperlinkButton.Content = CommentBlockResources.RepliesNavigationStringFormatted.Format(viewModel.Replies!.Count), TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                block.OpenRepliesHyperlinkButton.Content = CommentBlockResources.EmptyRepliesNavigationString;
            }

            block.PosterPersonPicture.ProfilePicture = await viewModel.GetAvatarSource();
            block.PosterTextBlock.Text = viewModel.Poster;
            block.PostDateTextBlock.Text = viewModel.PostDate.ToString("yyyy-MM-dd dddd");
            block.CommentContent.Visibility = (!viewModel.IsStamp).ToVisibility();
            block.StickerImageContent.Visibility = viewModel.IsStamp.ToVisibility();
            block.DeleteReplyHyperlinkButton.Visibility = (viewModel.PosterId == App.AppViewModel.PixivUid).ToVisibility();
            if (viewModel.IsStamp)
            {
                block.StickerImageContent.Source = viewModel.StampSource is { } url
                    ? await App.AppViewModel.MakoClient.DownloadSoftwareBitmapSourceResultAsync(url).GetOrElseAsync(await AppContext.GetNotAvailableImageAsync())
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

        private void OpenRepliesHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _repliesHyperlinkButtonTapped?.Invoke(sender, e);
        }

        private void DeleteReplyHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _deleteHyperlinkButtonTapped?.Invoke(sender, e);
        }
    }
}