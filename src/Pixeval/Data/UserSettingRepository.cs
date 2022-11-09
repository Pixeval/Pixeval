using Pixeval.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB.Async;

namespace Pixeval.Data
{
    internal class UserSettingRepository: BaseRepository<UserSetting>
    {
        public UserSettingRepository(ILiteDatabaseAsync database) : base(database)
        {
        }
    }
}
