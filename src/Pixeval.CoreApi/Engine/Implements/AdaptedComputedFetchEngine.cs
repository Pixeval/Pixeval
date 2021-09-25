#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/AdaptedComputedFetchEngine.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements
{
    /// <summary>
    ///     This class aims to hold an already computed <see cref="IFetchEngine{E}" />, it delegates
    ///     all of its property and methods to an inner <see cref="IEnumerable{T}" />, this class is
    ///     only supposed to be used by caching systems
    /// </summary>
    /// <typeparam name="T">The type of the results of the <see cref="IFetchEngine{E}" /></typeparam>
    [PublicAPI]
    public class AdaptedComputedFetchEngine<T> : IFetchEngine<T>
    {
        private readonly IEnumerable<T> _outer;

        /// <summary>
        ///     Creates an <see cref="AdaptedComputedFetchEngine{T}" /> that delegates all of its
        ///     property and methods to
        ///     <param name="outer"></param>
        /// </summary>
        /// <param name="outer">The <see cref="IEnumerable{T}" /> that is going to be delegated</param>
        public AdaptedComputedFetchEngine(IEnumerable<T> outer)
        {
            _outer = outer;
        }

        public MakoClient MakoClient => throw new NotSupportedException();

        // The 'AdaptedFetchEngine' is specialized for an "already computed" 'IFetchEngine'
        // which means its lifetime had been ended but computation result is cached into this
        // class, so the 'EngineHandle' that is used to track its lifetime is useless here
        public EngineHandle EngineHandle => throw new NotSupportedException();

        public int RequestedPages { get; set; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return new AdaptedAsyncEnumerator<T>(_outer.GetEnumerator(), cancellationToken);
        }
    }
}