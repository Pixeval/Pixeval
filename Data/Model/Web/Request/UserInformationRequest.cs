using Refit;

namespace Pixeval.Data.Model.Web.Request
{
    public class UserInformationRequest
    {
        [AliasAs("user_id")]
        public string Id { get; set; }

        [AliasAs("filter")]
        public string Filter { get; set; } = "for_ios";
    }
}