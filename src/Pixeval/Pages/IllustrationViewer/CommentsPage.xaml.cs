using System.Collections.Generic;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Net.Response;
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
            CommentsList.ItemsSource = new IncrementalLoadingCollection<CommentsIncrementalSource, CommentBlockViewModel>(new CommentsIncrementalSource(engine), 30);
        }
    }
}
