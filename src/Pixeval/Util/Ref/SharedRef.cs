using System;
using System.Collections.Generic;

namespace Pixeval.Util.Ref;

public class SharedRef<T>
{
    private readonly HashSet<object> _keys = new();

    public T Value { get; init; }

    public SharedRef(T value, object key)
    {
        Value = value;
        _ = _keys.Add(key);
    }

    public bool IsDisposed { get; private set; }

    public bool TryDispose(object key)
    {
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

    public SharedRef<T> MakeShared(object key)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(SharedRef<T>));
        _ = _keys.Add(key);
        return this;
    }
}
