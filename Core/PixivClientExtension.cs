using System.Collections.Generic;
using System.Linq;
using Pzxlane.Data.Model.ViewModel;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;
using Pzxlane.Objects;

namespace Pzxlane.Core
{
    public static class PixivClientExtension
    {
        public static void PostFavoriteAsync(this PixivClient _, Illustration illustration)
        {
            illustration.IsLiked = true;
            HttpClientFactory.AppApiService.AddBookmark(new AddBookmarkRequest {Id = illustration.Id});
        }

        public static void RemoveFavoriteAsync(this PixivClient _, Illustration illustration)
        {
            illustration.IsLiked = false;
            HttpClientFactory.AppApiService.DeleteBookmark(illustration.Id);
        }

        public static async IAsyncEnumerable<UserNavResponse.UserPreview> QueryUsers(this PixivClient _, string keyword)
        {
            for (var i = 0; ; i += 30)
            {
                var users = (await HttpClientFactory.AppApiService.GetUserNav(keyword, i)).UserPreviews;

                if (!users.Any()) yield break;

                foreach (var usersUserPreview in users)
                {
                    yield return usersUserPreview;
                }
            }
        }

        private static void DownloadIllustration(Illustration illustration, string path, Dictionary<string, string> header)
        {
            AsyncIO.DownloadFile(illustration.Origin, path, header);
        }

        private static void DownloadManga(Illustration illustration, string path, Dictionary<string, string> header)
        {
            if (illustration.Type == Illustration.IllustType.Manga)
            {

            }
        }
    }
}