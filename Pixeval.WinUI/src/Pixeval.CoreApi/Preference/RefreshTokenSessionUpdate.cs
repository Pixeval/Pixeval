using System.Threading.Tasks;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Preference
{
    public class RefreshTokenSessionUpdate : ISessionUpdate
    {
        public async Task<Session> Refresh(MakoClient makoClient)
        {
            return (await makoClient.Resolve<IAuthEndPoint>().Refresh(new RefreshSessionRequest(makoClient.Session.RefreshToken)))
                .ToSession().With(makoClient.Session);
        }
    }
}