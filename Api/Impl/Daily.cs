using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pzxlane.Api.Supplier;
using Pzxlane.Data.Model.Web;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Protocol;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;
using Refit;

namespace Pzxlane.Api.Impl
{
    public class Daily : IRecursiveContentSupplier<RankingResponse.Illust, RankingResponse>
    {
        private string link;

        public Daily()
        {
            link = "https://app-api.pixiv.net/v1/illust/recommended?content_type=illust&include_ranking_label=true&filter=for_ios";
        }

        public string GetIllustId(RankingResponse.Illust entity)
        {
            return entity.Id.ToString();
        }

        public async Task<IEnumerable<RankingResponse.Illust>> GetIllusts(object param)
        {
            if (Context == null)
            {
                Context = await HttpClientFactory.AppApiService.GetRanking(new RankingRequest());
            }
            else
            {
                var minBookmarksId = Regex.Match(link, "min_bookmark_id_for_recent_illust=(?<minBookmarkId>\\d+)").Groups["minBookmarkId"].Value;
                var maxBookmarksId = Regex.Match(link, "max_bookmark_id_for_recent_illust=(?<maxBookmarkId>\\d+)").Groups["maxBookmarkId"].Value;

                Context = await HttpClientFactory.AppApiService.GetRanking(new RankingRequest {MinBookmarkId = minBookmarksId, MaxBookmarkId = maxBookmarksId});
            }

            return Context.Illusts;
        }

        public RankingResponse Context { get; private set; }

        public bool GetCondition(RankingResponse param)
        {
            return link == null;
        }

        public void UpdateCondition(RankingResponse param)
        {
            link = Context.NextUrl.ToString();
        }
    }
}