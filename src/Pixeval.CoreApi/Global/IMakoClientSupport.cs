#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/IMakoClientSupport.cs
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

using JetBrains.Annotations;

namespace Pixeval.CoreApi.Global
{
    /// <summary>
    ///     Indicates that the each of its implementation contains a <see cref="MakoClient" />
    ///     that is to be used as a context provider, whereby "context provider" mostly refers to
    ///     the properties that are required when performing some context-aware tasks, such as the
    ///     access token while sending a request to app-api.pixiv.net
    /// </summary>
    [PublicAPI]
    public interface IMakoClientSupport
    {
        /// <summary>
        ///     The <see cref="MakoClient" /> that tends to be used as a context provider
        /// </summary>
        MakoClient MakoClient { get; }
    }
}