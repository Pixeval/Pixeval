using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CommunityToolkit.IncrementalLoadingCollection;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public class CommentsIncrementalSource : IIncrementalSource<CommentBlockViewModel>
    {
        private readonly IAsyncEnumerable<CommentBlockViewModel?> _source;

        public CommentsIncrementalSource(IAsyncEnumerable<CommentBlockViewModel?> source)
        {
            _source = source;
        }

        public async Task<IEnumerable<CommentBlockViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new())
        {
            return (await _source.Skip(pageIndex * pageSize).Take(pageSize).ToArrayAsync(cancellationToken))!;
        }
    }
}