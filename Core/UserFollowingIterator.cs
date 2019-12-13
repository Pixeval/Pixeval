using System.Collections.Generic;
using System.Linq;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class UserFollowingIterator
    {
        private readonly string userId;

        private string url;

        public UserFollowingIterator(string userId)
        {
            this.userId = userId;
            url = $"https://app-api.pixiv.net/v1/user/following?user_id={userId}&restrict=public";
        }

        public async IAsyncEnumerable<UserPreview> GetUserPreviews()
        {
            var client = HttpClientFactory.PixivApi(ProtocolBase.AppApiBaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Identity.Global.AccessToken}");

            while (!url.IsNullOrEmpty())
            {
                var response = (await client.GetStringAsync(url)).FromJson<FollowingResponse>();
                foreach (var preview in response.UserPreviews)
                    yield return new UserPreview
                    {
                        Thumbnails = preview.Illusts.Select(i => i.ImageUrls.SquareMedium).ToArray(),
                        UserId = preview.User.Id.ToString(),
                        UserName = preview.User.Name,
                        Avatar = preview.User.ProfileImageUrls.Medium
                    };

                url = response.NextUrl;
            }
        }
    }
}