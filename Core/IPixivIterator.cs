using System.Collections.Generic;
using System.Threading.Tasks;
using Pzxlane.Data.Model.ViewModel;

namespace Pzxlane.Core
{
    public interface IPixivIterator
    {
        bool HasNext();

        IAsyncEnumerable<Illustration> MoveNextAsync();
    }
}