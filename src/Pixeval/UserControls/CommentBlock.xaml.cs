using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class CommentBlock
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CommentBlockViewModel),
            typeof(CommentBlock),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, PropertyChangedCallback));

        private static async void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var block = (CommentBlock) d;
            if (e.NewValue is not CommentBlockViewModel viewModel)
            {
                return;
            }

            try
            {
                if (viewModel.HasReplies)
                {
                    _ = viewModel.LoadRepliesAsync().ContinueWith(_ => block.OpenRepliesHyperlinkButton.Content = CommentBlockResources.RepliesNavigationStringFormatted.Format(viewModel.Replies!.Count), TaskScheduler.FromCurrentSynchronizationContext());
                }

                block.PosterPersonPicture.ProfilePicture = await viewModel.GetAvatarSource();
                block.PosterTextBlock.Text = viewModel.Poster;
                block.PostDateTextBlock.Text = viewModel.PostDate.ToString(CultureInfo.CurrentUICulture);
                block.OpenRepliesHyperlinkButton.Visibility = viewModel.HasReplies.ToVisibility();
                block.CommentContent.Visibility = (!viewModel.IsStamp).ToVisibility();
                block.StickerImageContent.Visibility = viewModel.IsStamp.ToVisibility();
                if (viewModel.IsStamp)
                {
                    block.StickerImageContent.Source = viewModel.StampSource is { } url
                        ? await App.MakoClient.DownloadSoftwareBitmapSourceResultAsync(url).GetOrElseAsync(await AppContext.GetNotAvailableImageAsync())
                        : await AppContext.GetNotAvailableImageAsync();
                }
                else
                {
                    block.CommentContent.Blocks.Add(await viewModel.GetReplyContentParagraphAsync());
                }
            }
            catch (Exception exception)
            {
                Debugger.Break();
            }
        }

        public CommentBlockViewModel ViewModel
        {
            get => (CommentBlockViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public CommentBlock()
        {
            InitializeComponent();
        }
    }
}