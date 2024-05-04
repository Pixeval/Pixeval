#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2024 Pixeval.Utilities/AsyncLazy.cs
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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Pixeval.Utilities;

public class AsyncLazy<T>
{
    /// <summary>
    /// we ensure that _factory when finished is set to null to allow garbage collector to clean up
    /// any referenced items
    /// </summary>
    private Func<Task<T>>? _factory;

    /// <summary>
    /// _value eventually stores the lazily created value. It is valid when _state = true.
    /// </summary>
    private T? _value;

    public AsyncLazy(T value)
    {
        _value = value;
        IsValueCreated = true;
    }

    public AsyncLazy(Func<Task<T>> valueFactory) => _factory = valueFactory;

    public bool IsValueCreated { get; private set; }

    public T Value => IsValueCreated ? _value! : throw new NullReferenceException();

    public ValueTask<T> ValueAsync => ValueAsyncInternal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<T> ValueAsyncInternal() => IsValueCreated ? _value! : await CreateValueAsync();

    private async Task<T> CreateValueAsync()
    {
        var factory = _factory;
        ArgumentNullException.ThrowIfNull(factory);
        _factory = null;

        _value = await factory();
        IsValueCreated = true;
        return _value;
    }
}
