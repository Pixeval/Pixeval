using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;

namespace Pixeval.Tests;

[TestClass]
public sealed class SourceLoadLifetimeTest
{
    [TestMethod]
    public void BeginLoad_AbandonsPreviousOperationWithoutCancellation()
    {
        using var lifetime = new SourceLoadLifetime();
        var previous = lifetime.BeginLoad();

        _ = lifetime.BeginLoad();

        Assert.IsFalse(previous.Token.IsCancellationRequested);
        var discardedSource = new DisposableSource();
        Assert.IsFalse(previous.TrySetSource(discardedSource));
        Assert.IsTrue(discardedSource.IsDisposed);
        Assert.IsFalse(previous.Token.IsCancellationRequested);
    }

    [TestMethod]
    public void Dispose_CancelsCurrentAndAbandonedOperations()
    {
        var lifetime = new SourceLoadLifetime();
        var abandoned = lifetime.BeginLoad();
        var current = lifetime.BeginLoad();

        lifetime.Dispose();

        Assert.IsTrue(abandoned.Token.IsCancellationRequested);
        Assert.IsTrue(current.Token.IsCancellationRequested);
    }

    private sealed class DisposableSource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
