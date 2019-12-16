using System.Collections.Generic;
using Pixeval.Data.ViewModel;

namespace Pixeval.Core
{
    public interface IPixivIterator
    {
        bool HasNext();

        IAsyncEnumerable<Illustration> MoveNextAsync();
    }
}