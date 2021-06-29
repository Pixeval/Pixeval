using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    public class UserSearchEngine : AbstractPixivFetchEngine<User>
    {
        private readonly string _keyword;
        private readonly TargetFilter _targetFilter;
        private readonly UserSortOption _userSortOption;
        
        public UserSearchEngine([NotNull] MakoClient makoClient, TargetFilter targetFilter, UserSortOption? userSortOption, string keyword, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _keyword = keyword;
            _targetFilter = targetFilter;
            _userSortOption = userSortOption ?? UserSortOption.DateDescending;
        }

        public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.User<UserSearchEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
                engine => $"https://app-api.pixiv.net/v1/search/user?filter={engine._targetFilter.GetDescription()}&word={engine._keyword}&sort={engine._userSortOption.GetDescription()}")!;
        }
    }
}