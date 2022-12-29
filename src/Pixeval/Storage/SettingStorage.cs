using Pixeval.Data;
using Pixeval.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Pixeval.Storage
{
    internal class SettingStorage
    {
        private readonly SessionStorage _sessionStorage;
        private readonly IRepository<UserSetting> _repository;

        public SettingStorage(SessionStorage sessionStorage, IRepository<UserSetting> dataStore)
        {
            _sessionStorage = sessionStorage;
            _repository = dataStore;
        }

        public async Task<UserSetting?> GetSettingAsync()
        {
            var session = await _sessionStorage.GetSessionAsync();
            if (session is not null)
            {
                return await _repository.SingleOrDefaultAsync(_ => _.UserId == session.UserId);
            }
            return null;
        }

        public Task UpdateSettingAsync(UserSetting setting)
        {
            return _repository.UpdateAsync(setting);
        }
    }
}
