using System.Threading.Tasks;
using Pzxlane.Data.Model.ViewModel;
using Pzxlane.Data.Model.Web.Delegation;

namespace Pzxlane.Api.Client
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


    }
}