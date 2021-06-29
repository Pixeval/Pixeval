using System;
using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    internal class AdaptedComputedFetchEngine<T> : IFetchEngine<T>
    {
        private readonly IEnumerable<T> _outer;

        public MakoClient MakoClient => throw new NotSupportedException();
        
        // The 'AdaptedFetchEngine' is specialized for an "already computed" 'IFetchEngine'
        // which means its lifetime had been ended but computation result is cached into this
        // class, so the 'EngineHandle' that is used to track its lifetime is useless here
        public EngineHandle EngineHandle => throw new NotSupportedException();
        
        public int RequestedPages { get; set; }
        
        public AdaptedComputedFetchEngine(IEnumerable<T> outer)
        {
            _outer = outer;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return new AdaptedAsyncEnumerator<T>(_outer.GetEnumerator(), cancellationToken);
        }
    }
}