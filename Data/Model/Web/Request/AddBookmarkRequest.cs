using Refit;

namespace Pzxlane.Data.Model.Web.Request
{
    public class AddBookmarkRequest
    {
        [AliasAs("restrict")]
        public string Restrict { get; set; } = "public";
        
        [AliasAs("illust_id")]
        public string Id { get; set; }
    }
}