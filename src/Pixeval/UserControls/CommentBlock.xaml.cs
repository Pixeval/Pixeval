using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
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

        private EventHandler<TappedRoutedEventArgs>? _repliesHyperlinkButtonTapped;

        public event EventHandler<TappedRoutedEventArgs> RepliesHyperlinkButtonTapped
        {
            add => _repliesHyperlinkButtonTapped += value;
            remove => _repliesHyperlinkButtonTapped -= value;
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
            block.PostDateTextBlock.Text = viewModel.PostDate.ToString(CultureInfo.CurrentUICulture);
            block.CommentContent.Visibility = (!viewModel.IsStamp).ToVisibility();
            block.StickerImageContent.Visibility = viewModel.IsStamp.ToVisibility();
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

        public CommentBlockViewModel ViewModel
        {
            get => (CommentBlockViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public CommentBlock()
        {
            InitializeComponent();
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
    }
}