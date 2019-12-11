using Refit;

namespace Pixeval.Data.Web.Request
{
    public class QueryWorksRequest
    {
        [AliasAs("page")]
        public int Offset { get; set; }

        [AliasAs("q")]
        public string Tag { get; set; }

        [AliasAs("per_page")]
        public int PerPage { get; set; } = 300;

        [AliasAs("period")]
        public string Period { get; set; } = "all";

        [AliasAs("order")]
        public string Order { get; set; } = "desc";

        [AliasAs("sort")]
        public string Sort { get; set; } = "date";

        [AliasAs("mode")]
        public string Mode { get; set; } = "exact_tag";
    }
}