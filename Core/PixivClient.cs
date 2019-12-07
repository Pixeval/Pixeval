using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Documents;
using Pzxlane.Caching.Persisting;
using Pzxlane.Data.Model.ViewModel;
using Pzxlane.Data.Model.Web;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;
using Pzxlane.Objects;
using Page = System.Collections.Generic.IEnumerable<Pzxlane.Data.Model.ViewModel.Illustration>;

namespace Pzxlane.Core
{
    public sealed class PixivClient
    {
        private static volatile PixivClient _instance;
        
        private static readonly object Locker = new object();

        public static PixivClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                            _instance = new PixivClient();
                    }
                }

                return _instance;
            }
        }
    }
}