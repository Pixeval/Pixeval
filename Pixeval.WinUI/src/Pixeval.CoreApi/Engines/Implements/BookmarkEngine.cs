using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    internal class BookmarkEngine : AbstractPixivFetchEngine<Illustration>
    {
        private readonly string _uid;
        private readonly PrivacyPolicy _privacyPolicy;
        private readonly TargetFilter _targetFilter;

        public BookmarkEngine(
            MakoClient makoClient,
            string uid, 
            PrivacyPolicy privacyPolicy,
            TargetFilter targetFilter,
            EngineHandle? engineHandle = null) : base(makoClient, engineHandle)
        {
            _uid = uid;
            _privacyPolicy = privacyPolicy;
            _targetFilter = targetFilter;
        }
        
        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.Illustration<BookmarkEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
                engine => $"/v1/user/bookmarks/illust?user_id={engine._uid}&restrict={engine._privacyPolicy.GetDescription()}&filter={engine._targetFilter.GetDescription()}")!;
        }
    }
}