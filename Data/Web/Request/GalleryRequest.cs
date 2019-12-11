using Refit;

namespace Pixeval.Data.Web.Request
{
    public class GalleryRequest
    {
        [AliasAs("user_id")]
        public string Id { get; set; }

        [AliasAs("restrict")]
        public string Restrict { get; set; } = "public";

        [AliasAs("filter")]
        public string Filter { get; set; } = "for_ios";

        [AliasAs("max_bookmark_id")]
        public string MaxBookmarkId { get; set; } = null;
    }
}