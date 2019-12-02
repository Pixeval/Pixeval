using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pzxlane.Api.Supplier;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;

namespace Pzxlane.Api.Impl
{
    public class Favorite : IRecursiveContentSupplier<FavoriteWorksResponse.Illust, FavoriteWorksResponse>
    {
        private readonly string userId;

        private string link;

        public Favorite(string userId)
        {
            this.userId = userId;
            link = $"https://app-api.pixiv.net/v1/user/bookmarks/illust?user_id={userId}&restrict=public&filter=for_ios";
        }

        public string GetIllustId(FavoriteWorksResponse.Illust entity) => entity.Id.ToString();

        public async Task<IEnumerable<FavoriteWorksResponse.Illust>> GetIllusts(object param)
        {
            if (Context == null)
            {
                Context = await HttpClientFactory.AppApiService.GetFavoriteWorks(new FavoriteWorkRequest {Id = userId});
            }
            else
            {
                var maxBookmarkId = Regex.Match(link, "max_bookmark_id=(?<maxBookmarkId>\\d+)").Groups["maxBookmarkId"].Value;

                Context = await HttpClientFactory.AppApiService.GetFavoriteWorks(new FavoriteWorkRequest { Id = userId, MaxBookmarkId = maxBookmarkId });
            }

            return Context.Illusts;
        }

        public FavoriteWorksResponse Context { get; private set; }

        public bool GetCondition(FavoriteWorksResponse param)
        {
            return link == null;
        }

        public void UpdateCondition(FavoriteWorksResponse param)
        {
            link = Context.NextUrl.ToString();
        }
    }
}