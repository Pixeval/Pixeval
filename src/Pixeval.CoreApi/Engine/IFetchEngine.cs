#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/IFetchEngine.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using Pixeval.CoreApi.Global;

namespace Pixeval.CoreApi.Engine;

/// <summary>
///     An highly abstracted fetch engine that fetches pages and yields results asynchronously
///     <para>
///         Just like a fetch engine, it continuously fetches pages, and each page may contains either multiple
///         result entries or an error response, at each iteration, it fetches a single page and tries to
///         deserialize its content into a list of result entries or stops and reports the iteration is over
///     </para>
/// </summary>
/// <typeparam name="E">The type of the results of the <see cref="IFetchEngine{E}" /></typeparam>
public interface IFetchEngine<out E> : IAsyncEnumerable<E>, IMakoClientSupport, IEngineHandleSource
{
    /// <summary>
    ///     How many pages have been fetches
    /// </summary>
    int RequestedPages { get; set; }
}
