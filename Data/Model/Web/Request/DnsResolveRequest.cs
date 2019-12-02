using Refit;

namespace Pzxlane.Data.Model.Web.Request
{
    public class DnsResolveRequest
    {
        [AliasAs("ct")]
        public string Ct { get; set; }

        [AliasAs("name")]
        public string Name { get; set; }

        [AliasAs("type")]
        public string Type { get; set; }

        [AliasAs("do")]
        public string Do { get; set; }

        [AliasAs("cd")]
        public string Cd { get; set; }
    }
}