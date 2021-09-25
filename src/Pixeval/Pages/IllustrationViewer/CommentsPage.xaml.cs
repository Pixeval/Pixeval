using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Messages;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class CommentsPage
    {
        public CommentsPage()
        {
            InitializeComponent();
        }

        public override void Prepare(NavigationEventArgs e)
        {
            var engine = (IAsyncEnumerable<IllustrationCommentsResponse.Comment>) e.Parameter;
            CommentList.ItemsSource = new IncrementalLoadingCollection<CommentsIncrementalSource, CommentBlockViewModel>(
                new CommentsIncrementalSource(engine.Select(c => new CommentBlockViewModel(c))), 
                30);
        }

        private void CommentList_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new CommentRepliesHyperlinkButtonTappedMessage(sender));
        }
    }
}
