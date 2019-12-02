using Refit;

namespace Pzxlane.Data.Model.Web.Request
{
    public class UploadsRequest
    {
        [AliasAs("uid")]
        public string Uid { get; set; }

        [AliasAs("page")]
        public int Page { get; set; }

        [AliasAs("image_sizes")]
        public string ImageSizes { get; set; } = "px_128x128,px_480mw,large";

        [AliasAs("publicity")]
        public string Publicity { get; set; } = "public";

        [AliasAs("per_page")]
        public int PerPage { get; set; } = 60;
    }
}