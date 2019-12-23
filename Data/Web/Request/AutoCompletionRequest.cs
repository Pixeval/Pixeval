using Refit;

namespace Pixeval.Data.Web.Request
{
    public class AutoCompletionRequest
    {
        [AliasAs("merge_plain_keyword_results=true")]
        public bool MergePlainKeywordResult { get; set; } = true;

        [AliasAs("word")]
        public string Word { get; set; }
    }
}