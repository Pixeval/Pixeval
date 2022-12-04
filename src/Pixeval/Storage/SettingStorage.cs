using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi;
using Pixeval.Data;
using Pixeval.Models;

namespace Pixeval.Storage
{
    internal class SettingStorage
    {
        private readonly SessionStorage _sessionStorage;
        private readonly IBaseRepository<UserSetting> _userSettingRepository;

        public SettingStorage(SessionStorage sessionStorage, IBaseRepository<UserSetting> userSettingRepository)
        {
            _sessionStorage = sessionStorage;
            _userSettingRepository = userSettingRepository;
        }

        public async Task<UserSetting?> GetSettingAsync()
        {
            var session = await _sessionStorage.GetSessionAsync();
            if (session is not null)
            {
                await _userSettingRepository.Collection.FindOneAsync(_ => _.UserId == session.UserId);
            }
            return null;
        }

        public Task UpdateSettingAsync(UserSetting setting)
        {
            return _userSettingRepository.UpdateAsync(setting);
        }
    }
}
