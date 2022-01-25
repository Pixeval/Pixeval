#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2022 Pixeval.CoreApi/ComputedFetchEngine.cs
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
using System.Threading;

namespace Pixeval.CoreApi.Engine;

public class ComputedFetchEngine<T> : IFetchEngine<T>
{
    private readonly IAsyncEnumerable<T> _result;

    public ComputedFetchEngine(IAsyncEnumerable<T> result, MakoClient makoClient, EngineHandle engineHandle)
    {
        _result = result;
        MakoClient = makoClient;
        EngineHandle = engineHandle;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return _result.GetAsyncEnumerator(cancellationToken);
    }

    public MakoClient MakoClient { get; }

    public EngineHandle EngineHandle { get; }

    /// <summary>
    /// The <see cref="RequestedPages"/> in <see cref="ComputedFetchEngine{T}"/> should always returns -1
    /// </summary>
    public int RequestedPages { get; set; } = -1;
}