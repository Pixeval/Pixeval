using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PInvoke;
using Pixeval.CoreApi;
using Pixeval.Data;
using Pixeval.Models;

namespace Pixeval.Storage
{
    internal class SessionStorage : AbstractSessionStorage
    {
        private string? _userId;
        private readonly IBaseRepository<UserSession> _sessionRepository;

        public SessionStorage(IBaseRepository<UserSession> sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public override async Task<UserSession?> GetSessionAsync(string? userId = null)
        {
#nullable disable
            if (userId is not null)
            {
                return await _sessionRepository.Collection.FindOneAsync(_ => _.UserId == userId);
            }
            return await _sessionRepository.Collection.Query().OrderByDescending(_ => _.Updated).FirstOrDefaultAsync();
#nullable restore
        }

        public override async Task SetSessionAsync(string userId, string refreshToken, string accessToken)
        {
            UserSession session;
            if (_userId is not null)
            {
                session = await _sessionRepository.Collection.FindOneAsync(_ => _.UserId == _userId);
                if (session is not null)
                {
                    session = session with { Updated = DateTimeOffset.Now, RefreshToken = refreshToken };
                    await _sessionRepository.UpdateAsync(session);
                }
            }
            session = new UserSession(userId, refreshToken, accessToken, DateTimeOffset.Now);
            await _sessionRepository.CreateAsync(session);
        }

        public override Task ClearSessionAsync(string userId)
        {
            return _sessionRepository.Collection.DeleteManyAsync(_ => _.UserId == userId);
        }
    }
}
