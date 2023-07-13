using System;

namespace Pixeval.Util;

public record SharedRef<T>(T Value) : IDisposable where T : class, IDisposable
{
    private int _refCount = 1;

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        --_refCount;
        if (_refCount > 0)
            return;
        Value.Dispose();
        IsDisposed = true;
    }

    public void DisposeForce()
    {
        _refCount = 0;
        Value.Dispose();
        IsDisposed = true;
    }

    public SharedRef<T> MakeShared()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(SharedRef<T>));
        ++_refCount;
        return this;
    }
}
