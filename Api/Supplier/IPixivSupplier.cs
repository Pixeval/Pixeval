using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pzxlane.Api.Supplier
{
    public interface IPixivSupplier<TEntity, out TEntityList>
    {
        string GetIllustId(TEntity entity);

        Task<IEnumerable<TEntity>> GetIllusts(object param);

        TEntityList Context { get; }
    }
}