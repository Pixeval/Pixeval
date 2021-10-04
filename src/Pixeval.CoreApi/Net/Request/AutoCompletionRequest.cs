using Refit;

namespace Pixeval.CoreApi.Net.Request
{
    public class AutoCompletionRequest
    {
        [AliasAs("merge_plain_keyword_results=true")]
        public bool MergePlainKeywordResult { get; }

        [AliasAs("word")]
        public string Word { get; }

        public AutoCompletionRequest(string word)
        {
            MergePlainKeywordResult = true;
            Word = word;
        }
    }
}