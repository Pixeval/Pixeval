using System.Collections.Generic;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;

namespace Pixeval.Core
{
    public class UserInformationIterator
    {
        private readonly string keyword;

        private int currentIndex = 1;

        public UserInformationIterator(string keyword)
        {
            this.keyword = keyword;
        }

        public async IAsyncEnumerable<UserNavResponse.UserPreview> GetUserNav()
        {
            while (true)
            {
                var users = (await HttpClientFactory.AppApiService.GetUserNav(keyword, currentIndex++)).UserPreviews;
                if (users.IsNullOrEmpty())
                    yield break;

                foreach (var preview in users) yield return preview;
            }
        }
    }
}