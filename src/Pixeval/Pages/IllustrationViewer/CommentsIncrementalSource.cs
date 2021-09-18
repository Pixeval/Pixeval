using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Common.Collections;
using Pixeval.CoreApi.Net.Response;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public class CommentsIncrementalSource : IIncrementalSource<CommentBlockViewModel>
    {
        private readonly IAsyncEnumerable<IllustrationCommentsResponse.Comment?> _source;

        public CommentsIncrementalSource(IAsyncEnumerable<IllustrationCommentsResponse.Comment?> source)
        {
            _source = source;
        }

        public async Task<IEnumerable<CommentBlockViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new())
        {
            return await _source.Skip(pageIndex * pageSize).Take(pageSize).Select(c => new CommentBlockViewModel(c!)).ToArrayAsync(cancellationToken);
        }
    }
}