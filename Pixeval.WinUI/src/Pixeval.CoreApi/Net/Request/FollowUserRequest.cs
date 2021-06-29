using Refit;

namespace Pixeval.CoreApi.Net.Request
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    internal class FollowUserRequest
    {
        [AliasAs("user_id")]
        public string Id { get; }

        [AliasAs("restrict")]
        public string Restrict { get; }

        public FollowUserRequest(string id, string restrict)
        {
            Id = id;
            Restrict = restrict;
        }
    }
}