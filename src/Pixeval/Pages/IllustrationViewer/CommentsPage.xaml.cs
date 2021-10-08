using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CommunityToolkit.IncrementalLoadingCollection;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Messages;
using Pixeval.UserControls;
using Pixeval.Util.IO;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class CommentsPage
    {
        private string? _illustId;

        public CommentsPage()
        {
            InitializeComponent();
        }

        public override void OnPageActivated(NavigationEventArgs e)
        {
            var (engine, illustId) = ((IAsyncEnumerable<Comment>, string)) e.Parameter;
            _illustId = illustId;
            CommentList.ItemsSource = new IncrementalLoadingCollection<CommentsIncrementalSource, CommentBlockViewModel>(
                new CommentsIncrementalSource(engine.Select(c => new CommentBlockViewModel(c, illustId))), 
                30);
        }

        private void CommentList_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new CommentRepliesHyperlinkButtonTappedMessage(sender));
        }

        private async void ReplyBar_OnSendButtonTapped(object? sender, SendButtonTappedEventArgs e)
        {
            using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment, 
                ("illust_id", _illustId!),
                ("comment", e.ReplyContentRichEditBoxStringContent));

            await AddComment(result);
        }

        private async void ReplyBar_OnStickerTapped(object? sender, StickerTappedEventArgs e)
        {
            using var result = await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.AppApi).PostFormAsync(CommentBlockViewModel.AddCommentUrlSegment, 
                ("illust_id", _illustId!),
                ("stamp_id", e.StickerViewModel.StickerId.ToString()));

            await AddComment(result);
        }

        private async Task AddComment(HttpResponseMessage postCommentResponse)
        {
            if (CommentList.ItemsSource is IEnumerable<object> enumerable && !enumerable.Any())
            {
                CommentList.ItemsSource = new ObservableCollection<CommentBlockViewModel>();
            }

            if (postCommentResponse.IsSuccessStatusCode)
            {
                var response = await postCommentResponse.Content.ReadFromJsonAsync<PostCommentResponse>();
                ((ObservableCollection<CommentBlockViewModel>) CommentList.ItemsSource).Insert(0, new CommentBlockViewModel(response?.Comment!, _illustId!));
            }
        }
    }
}
