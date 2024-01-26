#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/AdaptedComputedFetchEngine.cs
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

using System;
using System.Collections.Generic;
using System.Threading;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

/// <summary>
/// This class aims to hold an already computed <see cref="IFetchEngine{E}" />, it delegates
/// all of its property and methods to an inner <see cref="IEnumerable{T}" />, this class is
/// only supposed to be used by caching systems
/// </summary>
/// <typeparam name="T">The type of the results of the <see cref="IFetchEngine{E}" /></typeparam>
/// <remarks>
/// Creates an <see cref="AdaptedComputedFetchEngine{T}" /> that delegates all of its
/// property and methods to
/// <param name="outer"></param>
/// </remarks>
/// <param name="outer">The <see cref="IEnumerable{T}" /> that is going to be delegated</param>
public class AdaptedComputedFetchEngine<T>(IEnumerable<T> outer) : IFetchEngine<T>
{
    private readonly IEnumerable<T> _outer = outer;

    public MakoClient MakoClient => throw new NotSupportedException();

    // The 'AdaptedFetchEngine' is specialized for an "already computed" 'IFetchEngine'
    // which means its lifetime had been ended but computation result is cached into this
    // class, so the 'EngineHandle' that is used to track its lifetime is useless here
    public EngineHandle EngineHandle => throw new NotSupportedException();

    public int RequestedPages { get; set; }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new AdaptedAsyncEnumerator<T>(_outer.GetEnumerator(), cancellationToken);
    }
}
