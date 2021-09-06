#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/AbstractPixivFetchEngine.cs
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

namespace Pixeval.CoreApi.Engine
{
    /// <summary>
    ///     A <see cref="IFetchEngine{E}" /> that specialized for Pixiv, it holds an <see cref="MakoClient" />
    ///     and a <see cref="EngineHandle" /> to manage its lifetime
    /// </summary>
    /// <typeparam name="E">
    ///     <inheritdoc cref="IFetchEngine{E}" />
    /// </typeparam>
    public abstract class AbstractPixivFetchEngine<E> : IFetchEngine<E>
    {
        protected AbstractPixivFetchEngine(MakoClient makoClient, EngineHandle? engineHandle)
        {
            MakoClient = makoClient;
            EngineHandle = engineHandle ?? new EngineHandle(Guid.NewGuid());
        }

        public abstract IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = new()); // the 'CancellationToken' is no longer useful, we use 'EngineHandle' to track the lifetime

        /// <summary>
        ///     The <see cref="MakoClient" /> that owns this <see cref="IFetchEngine{E}" />, it
        ///     shares its context such as <see cref="CoreApi.MakoClient.Configuration" /> with current
        ///     <see cref="IFetchEngine{E}" /> to provides the required fields when the <see cref="IFetchEngine{E}" />
        ///     performing its task
        /// </summary>
        public MakoClient MakoClient { get; }

        /// <summary>
        ///     How many pages have been fetched
        /// </summary>
        public int RequestedPages { get; set; }

        /// <summary>
        ///     The <see cref="EngineHandle" /> used to manage the lifetime of <see cref="IFetchEngine{E}" />
        /// </summary>
        public EngineHandle EngineHandle { get; }
    }
}