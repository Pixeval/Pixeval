using System.Collections.Generic;
using Pixeval.Data.Model.ViewModel;

namespace Pixeval.Core
{
    public interface IPixivIterator
    {
        bool HasNext();

        IAsyncEnumerable<Illustration> MoveNextAsync();
    }
}