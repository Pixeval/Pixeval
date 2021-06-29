using Refit;

namespace Pixeval.CoreApi.Net.Request
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    internal class AddBookmarkRequest
    {
        [AliasAs("restrict")]
        public string Restrict { get; }
        
        [AliasAs("illust_id")]
        public string Id { get; }

        public AddBookmarkRequest(string restrict, string id)
        {
            Restrict = restrict;
            Id = id;
        }
    }
}