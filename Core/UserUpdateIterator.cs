using System.Collections.Generic;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Core
{
    public class UserUpdateIterator : IPixivIterator<Illustration>
    {
        private UserUpdateResponse context;

        public bool HasNext()
        {
            if (context == null) return true;

            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            const string url = "https://app-api.pixiv.net/v2/illust/follow?restrict=public";
            var response = (await HttpClientFactory.AppApiHttpClient.GetStringAsync(context == null ? url : context.NextUrl)).FromJson<UserUpdateResponse>();

            if (response.Illusts.IsNullOrEmpty() && context == null) throw new QueryNotRespondingException();

            context = response;
            foreach (var illust in context.Illusts)
            {
                var res = await PixivHelper.IllustrationInfo(illust.Id.ToString());
                if (res != null) yield return res;
            }
        }
    }
}