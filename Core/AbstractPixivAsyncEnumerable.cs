using System.Collections.Generic;
using System.Threading;

namespace Pixeval.Core
{
    public abstract class AbstractPixivAsyncEnumerable<T> : IPixivAsyncEnumerable<T>
    {
        protected bool IsCancelled { get; private set; }

        public abstract SortOption SortOption { get; }

        public abstract int RequestedPages { get; protected set; }

        public abstract IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);

        public void Cancel()
        {
            IsCancelled = true;
        }

        public bool IsCancellationRequested()
        {
            return IsCancelled;
        }

        public void ReportRequestedPages()
        {
            RequestedPages++;
        }
    }
}