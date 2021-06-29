using Refit;

namespace Pixeval.CoreApi.Net.Request
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class SingleUserRequest
    {
        [AliasAs("user_id")]
        public string Id { get; }

        [AliasAs("filter")]
        public string Filter { get; }

        public SingleUserRequest(string id, string filter)
        {
            Id = id;
            Filter = filter;
        }
    }
}