using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;

namespace Pixeval.Core
{
    public static class PixivClientExtension
    {
        public static async void PostFavoriteAsync(this PixivClient _, Illustration illustration)
        {
            illustration.IsLiked = true;
            await HttpClientFactory.AppApiService.AddBookmark(new AddBookmarkRequest {Id = illustration.Id});
        }

        public static async void RemoveFavoriteAsync(this PixivClient _, Illustration illustration)
        {
            illustration.IsLiked = false;
            await HttpClientFactory.AppApiService.DeleteBookmark(new DeleteBookmarkRequest {IllustId = illustration.Id});
        }

        public static async IAsyncEnumerable<UserNavResponse.UserPreview> QueryUsers(this PixivClient _, string keyword)
        {
            for (var i = 0;; i += 30)
            {
                var users = (await HttpClientFactory.AppApiService.GetUserNav(keyword, i)).UserPreviews;

                if (!users.Any()) yield break;

                foreach (var usersUserPreview in users) yield return usersUserPreview;
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