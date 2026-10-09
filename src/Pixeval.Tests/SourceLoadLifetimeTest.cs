using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Controls;

namespace Pixeval.Tests;

[TestClass]
public sealed class SourceLoadLifetimeTest
{
    [TestMethod]
    public void BeginLoad_CancelsPreviousDecodeWithoutCancelingDownload()
    {
        using var lifetime = new SourceLoadLifetime();
        var previous = lifetime.BeginLoad();

        _ = lifetime.BeginLoad();

        Assert.IsFalse(previous.Token.IsCancellationRequested);
        Assert.IsTrue(previous.DecodeToken.IsCancellationRequested);
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
        Assert.IsTrue(abandoned.DecodeToken.IsCancellationRequested);
        Assert.IsTrue(current.DecodeToken.IsCancellationRequested);
    }

    [TestMethod]
    public void RejectedResultIsRemovedFromLifetimeWithoutCancelingCacheFill()
    {
        var lifetime = new SourceLoadLifetime();
        var obsolete = lifetime.BeginLoad();
        var current = lifetime.BeginLoad();

        Assert.IsTrue(obsolete.DecodeToken.IsCancellationRequested);
        Assert.IsFalse(obsolete.Token.IsCancellationRequested);
        Assert.IsFalse(obsolete.TrySetSource(new DisposableSource()));
        lifetime.Dispose();

        Assert.IsFalse(obsolete.Token.IsCancellationRequested);
        Assert.IsTrue(current.Token.IsCancellationRequested);
        Assert.IsTrue(current.DecodeToken.IsCancellationRequested);
    }

    [TestMethod]
    public void AbandonResultAndPageDisposalCompleteOnlyOnce()
    {
        for (var i = 0; i < 100; i++)
        {
            var completions = 0;
            var operation = new SourceLoadOperation(_ => Interlocked.Increment(ref completions));
            var source = new DisposableSource();
            Parallel.Invoke(operation.Abandon, () => operation.TrySetSource(source), operation.Dispose);

            Assert.AreEqual(1, completions);
            Assert.IsTrue(operation.DecodeToken.IsCancellationRequested);
            Assert.IsTrue(source.IsDisposed);
            operation.Abandon();
            operation.Dispose();
            Assert.AreEqual(1, completions);
        }
    }

    private sealed class DisposableSource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
