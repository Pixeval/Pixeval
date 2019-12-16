using System.Collections.Generic;
using System.Linq;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class UserPreviewIterator
    {
        private string url;

        public UserPreviewIterator(string keyword)
        {
            url = $"https://app-api.pixiv.net/v1/search/user?filter=for_android&word={keyword}";
        }

        public async IAsyncEnumerable<User> GetUserPreview()
        {
            var counter = 0;
            while (!url.IsNullOrEmpty())
            {
                if (counter > 10) yield break;

                var httpClient = HttpClientFactory.PixivApi(ProtocolBase.AppApiBaseUrl);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {Identity.Global.AccessToken}");

                var response = (await httpClient.GetStringAsync(url)).FromJson<UserNavResponse>();

                if (response.UserPreviews.IsNullOrEmpty() && counter++ == 0) throw new QueryNotRespondingException();

                url = response.NextUrl;

                foreach (var responseUserPreview in response.UserPreviews)
                    yield return new User
                    {
                        Avatar = responseUserPreview.User.ProfileImageUrls.Medium,
                        Thumbnails = responseUserPreview.Illusts.Select(i => i.ImageUrl.SquareMedium).ToArray(),
                        Id = responseUserPreview.User.Id.ToString(),
                        Name = responseUserPreview.User.Name
                    };
            }
        }
    }
}