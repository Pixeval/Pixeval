using Refit;

namespace Pixeval.Data.Model.Web.Request
{
    public class TokenRequest
    {
        [AliasAs("username")]
        public string Name { get; set; }

        [AliasAs("password")]
        public string Password { get; set; }

        [AliasAs("grant_type")]
        public string GrantType => "password";

        [AliasAs("client_id")]
        public string ClientId => "MOBrBDS8blbauoSck0ZfDbtuzpyT";

        [AliasAs("client_secret")]
        public string ClientSecret => "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";

        [AliasAs("get_secure_url")]
        public string GetSecureUrl => "1";
    }
}