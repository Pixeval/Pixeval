using System;
using System.Collections.Generic;

namespace Pixeval.Util.Ref;

public class SharedRef<T> where T : class, IDisposable
{
    private readonly HashSet<object> _keys = new();

    public T Value { get; init; }

    public SharedRef(T value, object key)
    {
        Value = value;
        _ = _keys.Add(key);
    }

    public bool IsDisposed { get; private set; }

    public bool Dispose(object key)
    {
        _ = _keys.Remove(key);
        if (_keys.Count > 0)
            return false;
        Value.Dispose();
        return IsDisposed = true;
    }

    public void DisposeForce()
    {
        _keys.Clear();
        Value.Dispose();
        IsDisposed = true;
    }

    public SharedRef<T> MakeShared(object key)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(SharedRef<T>));
        _ = _keys.Add(key);
        return this;
    }
}
