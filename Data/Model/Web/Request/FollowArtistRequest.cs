using Refit;

namespace Pzxlane.Data.Model.Web.Request
{
    public class FollowArtistRequest
    {
        [AliasAs("user_id")]
        public string Id { get; set; }

        [AliasAs("restrict")]
        public string Restrict { get; set; } = "public";
    }
}