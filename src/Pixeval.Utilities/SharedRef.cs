// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;

namespace Pixeval.Utilities;

public class SharedRef<T>
{
    private readonly HashSet<int> _keys = [];

    public T Value { get; init; }

    public SharedRef(T value, object key)
    {
        IdentifyKey(key);
        Value = value;
        _ = _keys.Add(key.GetHashCode());
    }

    public bool IsDisposed { get; private set; }

    public bool TryDispose<TKey>(TKey key) where TKey : class
    {
        IdentifyKey(key);
        _ = _keys.Remove(key.GetHashCode());
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
            ThrowUtils.Argument("Key must be a reference type and not a string.");
#endif
    }

    public SharedRef<T> MakeShared<TKey>(TKey key) where TKey : class
    {
        IdentifyKey(key);
        ObjectDisposedException.ThrowIf(IsDisposed, typeof(SharedRef<T>));
        _ = _keys.Add(key.GetHashCode());
        return this;
    }
}
