using Pixeval.CoreApi;
using Pixeval.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixeval.Storage;

internal class SessionStorage
{
    private string? _userId;
    private readonly IRepository<UserSession> _repository;

    public SessionStorage(IRepository<UserSession> repository)
    {
        _repository = repository;
    }

    public async Task<UserSession?> GetSessionAsync(string? userId = null)
    {
#nullable disable
        if (userId is not null)
        {
            return await _repository.SingleOrDefaultAsync(_ => _.UserId == userId);
        }
        return (await _repository.ListAsync()).MaxBy(_ => _.Updated);
#nullable restore
    }

    public async Task SetSessionAsync(string userId, string refreshToken, string accessToken)
    {
        UserSession? session;
        if (_userId is not null)
        {
            session = await _repository.SingleOrDefaultAsync(_ => _.UserId == _userId);
            if (session is not null)
            {
                session = session with { Updated = DateTimeOffset.Now, RefreshToken = refreshToken };
                await _repository.UpdateAsync(session);
            }
        }
        session = new UserSession(userId, refreshToken, accessToken, DateTimeOffset.Now);
        await _repository.AddAsync(session);
    }

    public async Task ClearSessionAsync(string userId)
    {
        if (await _repository.FirstOrDefaultAsync(_ => _.UserId == userId) is { } obj)
        {
            await _repository.DeleteAsync(obj);
        }

    }
}