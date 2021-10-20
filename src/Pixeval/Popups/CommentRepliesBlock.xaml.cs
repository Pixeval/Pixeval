using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.CommunityToolkit;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.UserControls;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.Popups
{
    public sealed partial class CommentRepliesBlock : IAppPopupContent
    {
        public Guid UniqueId { get; }

        public FrameworkElement UIContent => this;

        public static DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CommentRepliesBlockViewModel),
            typeof(CommentRepliesBlock),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ViewModelChangedCallback));

        public CommentRepliesBlockViewModel ViewModel
        {
            get => (CommentRepliesBlockViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public CommentRepliesBlock()
        {
            UniqueId = Guid.NewGuid();
            InitializeComponent();
        }

        private EventHandler<TappedRoutedEventArgs>? _closeButtonTapped;

        public event EventHandler<TappedRoutedEventArgs> CloseButtonTapped
        {
            add => _closeButtonTapped += value;
            remove => _closeButtonTapped -= value;
        }

        private static void ViewModelChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var block = (CommentRepliesBlock) d;
            var viewModel = (CommentRepliesBlockViewModel) e.NewValue;
            block.RepliesAreEmptyPanel.Visibility = (!viewModel.HasReplies).ToVisibility();
            block.CommentList.Visibility = viewModel.HasReplies.ToVisibility();
            if (viewModel.HasReplies && viewModel.Comment.Replies is { } rs)
            {
                block.CommentList.ItemsSource = rs;
            }
        }

        private void CommentRepliesBlock_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Focus the popup content so that the hot key for closing can work properly
            CloseButton.Focus(FocusState.Programmatic);
        }

        private void CloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _closeButtonTapped?.Invoke(sender, e);
        }

        private void CommentList_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
        {
            ReplyBar.FindDescendant<RichEditBox>()?.Focus(FocusState.Programmatic);
        }

        private async void ReplyBar_OnSendButtonTapped(object? sender, SendButtonTappedEventArgs e)
        {
            using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment,
                ("illust_id", ViewModel.Comment.IllustrationId),
                ("parent_comment_id", ViewModel.Comment.CommentId), 
                ("comment", e.ReplyContentRichEditBoxStringContent));

            await AddComment(result);
        }

        private async void ReplyBar_OnStickerTapped(object? sender, StickerTappedEventArgs e)
        {
            using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment, 
                ("illust_id", ViewModel.Comment.IllustrationId), 
                ("parent_comment_id", ViewModel.Comment.CommentId),
                ("stamp_id", e.StickerViewModel.StickerId.ToString()));

            await AddComment(result);
        }

        private async Task AddComment(HttpResponseMessage postCommentResponse)
        {
            RepliesAreEmptyPanel.Visibility = Visibility.Collapsed;
            CommentList.Visibility = Visibility.Visible;

            if (CommentList.ItemsSource is IEnumerable<object> enumerable && !enumerable.Any())
            {
                CommentList.ItemsSource = new ObservableCollection<CommentBlockViewModel>();
            }

            if (postCommentResponse.IsSuccessStatusCode)
            {
                var response = await postCommentResponse.Content.ReadFromJsonAsync<PostCommentResponse>();
                ((ObservableCollection<CommentBlockViewModel>) CommentList.ItemsSource).Insert(0, new CommentBlockViewModel(response?.Comment!, ViewModel.Comment.IllustrationId));
            }
        }
    }
}
