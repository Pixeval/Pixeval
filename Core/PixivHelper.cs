using System.Threading.Tasks;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Model.Web.Delegation;
using Pixeval.Data.Model.Web.Request;

namespace Pixeval.Core
{
    public class PixivHelper
    {
        public static async Task<Illustration> IllustrationInfo(string id)
        {
            var response = (await HttpClientFactory.PublicApiService.GetSingle(id)).ToResponse[0];

            if (response == null)
            {
                return null;
            }

            return new Illustration
            {
                Bookmark = (int) (response.Stats.FavoritedCount.Private + response.Stats.FavoritedCount.Public),
                Id = response.Id.ToString(),
                IsLiked = response.FavoriteId != 0,
                IsManga = response.IsManga,
                IsUgoira = response.Type == "ugoira",
                Origin = response.ImageUrls.Large,
                Tags = response.Tags.ToArray(),
                Thumbnail = response.ImageUrls.Medium,
                Title = response.Title,
                Type = Illustration.IllustType.Parse(response),
                UserName = response.User.Name,
                UserId = response.User.Id.ToString()
            };
        }

        public static async Task<int> GetUploadPagesCount(string uid)
        {
            return (int) (await HttpClientFactory.PublicApiService.GetUploads(uid, new UploadsRequest { Page = 1 }))
                .UploadPagination
                .Pages;
        }

        public static async Task<int> GetQueryPagesCount(string tag)
        {
            return (int)(await HttpClientFactory.PublicApiService.QueryWorks(new QueryWorksRequest { Tag = tag, Offset = 1 }))
                .QueryPagination
                .Pages;
        }
    }
}