#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/IFetchEngine.cs
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

using System.Collections.Generic;
using JetBrains.Annotations;
using Pixeval.CoreApi.Global;

namespace Pixeval.CoreApi.Engine
{
    /// <summary>
    ///     An highly abstracted fetch engine that fetches pages and yields results asynchronously
    ///     <para>
    ///         Just like a fetch engine, it continuously fetches pages, and each page may contains multiple
    ///         result entries, or an error response, at each iteration, it fetches one page and tries to
    ///         deserialize its content into a list of result entries, if an error response is occur, the
    ///         fetch engine stops and reports the iteration is over
    ///     </para>
    /// </summary>
    /// <typeparam name="E">The type of the results of the <see cref="IFetchEngine{E}" /></typeparam>
    [PublicAPI]
    public interface IFetchEngine<out E> : IAsyncEnumerable<E>, IMakoClientSupport, IEngineHandleSource
    {
        /// <summary>
        ///     How many pages have been fetches
        /// </summary>
        int RequestedPages { get; set; }
    }
}