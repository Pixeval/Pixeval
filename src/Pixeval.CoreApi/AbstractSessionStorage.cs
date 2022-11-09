using System.Threading.Tasks;

namespace Pixeval.CoreApi
{
    public abstract class AbstractSessionStorage
    {
        public abstract Task<UserSession?> GetSessionAsync(string? userId = null);

        public abstract Task SetSessionAsync(string userId, string refreshToken, string accessToken);

        public abstract Task ClearSessionAsync(string userId);
    }
}
