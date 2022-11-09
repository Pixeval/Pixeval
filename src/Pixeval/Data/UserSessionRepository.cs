using Pixeval.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB.Async;
using Pixeval.CoreApi;

namespace Pixeval.Data
{
    internal class UserSessionRepository: BaseRepository<UserSession>
    {
        public UserSessionRepository(ILiteDatabaseAsync database) : base(database)
        {
        }
    }
}
