using Refit;

namespace Pixeval.Data.Web.Request
{
    public class UnFollowArtistRequest
    {
        [AliasAs("user_id")]
        public string UserId { get; set; }
    }
}