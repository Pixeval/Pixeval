using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CommunityToolkit.IncrementalLoadingCollection
{
    /// <summary>
    /// This interface represents a data source whose items can be loaded incrementally.
    /// </summary>
    /// <typeparam name="TSource">Type of collection element.</typeparam>
    public interface IIncrementalSource<TSource>
    {
        /// <summary>
        /// This method is invoked every time the view need to show more items. Retrieves items based on <paramref name="pageIndex"/> and <paramref name="pageSize"/> arguments.
        /// </summary>
        /// <param name="pageIndex">
        /// The zero-based index of the page that corresponds to the items to retrieve.
        /// </param>
        /// <param name="pageSize">
        /// The number of <typeparamref name="TSource"/> items to retrieve for the specified <paramref name="pageIndex"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Used to propagate notification that operation should be canceled.
        /// </param>
        /// <returns>
        /// Returns a collection of <typeparamref name="TSource"/>.
        /// </returns>
        Task<IEnumerable<TSource>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken));
    }
}