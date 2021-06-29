using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Preference
{
    [PublicAPI]
    public interface ISessionUpdate
    {
        Task<Session> Refresh(MakoClient makoClient);
    }
}