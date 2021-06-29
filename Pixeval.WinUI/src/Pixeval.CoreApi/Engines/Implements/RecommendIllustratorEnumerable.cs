using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    internal class RecommendIllustratorEngine : AbstractPixivFetchEngine<User>
    {
        private readonly TargetFilter _targetFilter;

        public RecommendIllustratorEngine(MakoClient makoClient, TargetFilter targetFilter, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _targetFilter = targetFilter;
        }

        public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.User<RecommendIllustratorEngine>.WithInitialUrl(this, MakoApiKind.AppApi, engine => $"/v1/user/recommended?filter={engine._targetFilter.GetDescription()}")!;
        }
    }
}