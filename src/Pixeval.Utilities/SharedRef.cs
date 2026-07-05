// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Pixeval.Utilities;

public class SharedRef<T>
{
    private readonly Lock _gate = new();
    private readonly object _ownerMarker = new();
    private readonly ConditionalWeakTable<object, object> _keys = new();

    public T Value { get; init; }

    public SharedRef(T value, object key)
    {
        IdentifyKey(key);
        Value = value;
        _keys.Add(key, _ownerMarker);
    }

    public bool IsDisposed { get; private set; }

    public bool TryDispose<TKey>(TKey key) where TKey : class
    {
        IdentifyKey(key);
        if (IsDisposed)
            return false;

        lock (_gate)
        {
            if (IsDisposed)
                return false;

            if (!_keys.Remove(key) || HasOwners())
                return false;

            if (Value is IDisposable disposable)
                disposable.Dispose();
            return IsDisposed = true;
        }
    }

    public void DisposeForce()
    {
        if (IsDisposed)
            return;

        lock (_gate)
        {
            if (IsDisposed)
                return;

            _keys.Clear();
            if (Value is IDisposable disposable)
                disposable.Dispose();
            IsDisposed = true;
        }
    }

    public void IdentifyKey(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        // 判断key是不是引用类型
        var type = key.GetType();
        if (type.IsValueType || type == typeof(string))
            throw new ArgumentException("Key must be a reference type and not a string.");
    }

    public SharedRef<T> MakeShared<TKey>(TKey key) where TKey : class
    {
        IdentifyKey(key);
        ObjectDisposedException.ThrowIf(IsDisposed, typeof(SharedRef<T>));

        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, typeof(SharedRef<T>));
            if (!_keys.TryGetValue(key, out _))
                _keys.Add(key, _ownerMarker);
            return this;
        }
    }

    private bool HasOwners()
    {
        foreach (var _ in _keys)
        {
            return true;
        }

        return false;
    }
}
