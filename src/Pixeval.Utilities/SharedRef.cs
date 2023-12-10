#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2023 Pixeval.Utilities/SharedRef.cs
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

namespace Pixeval.Utilities;

public class SharedRef<T>
{
    private readonly HashSet<object> _keys = [];

    public T Value { get; init; }

    public SharedRef(T value, object key)
    {
        IdentifyKey(key);
        Value = value;
        _ = _keys.Add(key);
    }

    public bool IsDisposed { get; private set; }

    public bool TryDispose<TKey>(TKey key) where TKey : class
    {
        IdentifyKey(key);
        _ = _keys.Remove(key);
        if (_keys.Count > 0)
            return false;
        if (Value is IDisposable disposable)
            disposable.Dispose();
        return IsDisposed = true;
    }

    public void DisposeForce()
    {
        _keys.Clear();
        if (Value is IDisposable disposable)
            disposable.Dispose();
        IsDisposed = true;
    }

    public void IdentifyKey(object key)
    {
#if DEBUG
        // 判断key是不是引用类型
        var type = key.GetType();
        if (type.IsValueType || type == typeof(string))
            throw new ArgumentException("Key must be a reference type and not a string.");
#endif
    }

    public SharedRef<T> MakeShared<TKey>(TKey key) where TKey : class
    {
        IdentifyKey(key);
        ObjectDisposedException.ThrowIf(IsDisposed, typeof(SharedRef<T>));
        _ = _keys.Add(key);
        return this;
    }
}
