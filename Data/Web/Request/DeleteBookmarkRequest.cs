using Refit;

namespace Pixeval.Data.Web.Request
{
    public class DeleteBookmarkRequest
    {
        [AliasAs("illust_id")]
        public string IllustId { get; set; }
    }
}