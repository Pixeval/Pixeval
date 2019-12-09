using Refit;

namespace Pixeval.Data.Model.Web.Request
{
    public class RankingRequest
    {
        [AliasAs("content_type")]
        public string ContentType { get; set; } = "illust";

        [AliasAs("filter")]
        public string Filter { get; set; } = "for_ios";

        [AliasAs("min_bookmark_id_for_recent_illust")]
        public string MinBookmarkId { get; set; } = null;

        [AliasAs("max_bookmark_id_for_recommend")]
        public string MaxBookmarkId { get; set; } = null;
    }
}