// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mako;
using Mako.Engine;
using Mako.Global.Exception;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;

namespace Pixeval.Tests;

[TestClass]
public sealed class FetchEngineRetryHelperTest
{
    [TestMethod]
    public async Task StreamAsync_RetriesInterruptedEnumeratorWithoutStartingOver()
    {
        var engine = new InterruptingFetchEngine();
        var results = new List<int>();

        await foreach (var result in FetchEngineRetryHelper.StreamAsync(
                           engine,
                           static _ => TimeSpan.Zero))
            results.Add(result);

        Assert.AreSequenceEqual([1, 2], results);
        Assert.AreEqual(1, engine.EnumeratorCount);
        Assert.AreEqual(4, engine.MoveNextCount);
        Assert.IsTrue(engine.EngineHandle.IsCompleted);
    }

    [TestMethod]
    public async Task StreamAsync_CanCancelEnumerationWithoutCancellingSharedEngine()
    {
        var engine = new InterruptingFetchEngine();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _ = await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in FetchEngineRetryHelper.StreamAsync(
                               engine,
                               cancelEngineOnCancellation: false,
                               token: cancellationTokenSource.Token))
            {
            }
        });

        Assert.IsFalse(engine.EngineHandle.IsCancelled);
        Assert.AreEqual(1, engine.EnumeratorCount);
        Assert.AreEqual(0, engine.MoveNextCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_RetriesRateLimitedRequest()
    {
        var attemptCount = 0;

        var result = await FetchEngineRetryHelper.ExecuteAsync(
            _ => ++attemptCount is 1
                ? Task.FromException<int>(new MakoNetworkException("test", false, null, 429))
                : Task.FromResult(42),
            static _ => TimeSpan.Zero);

        Assert.AreEqual(42, result);
        Assert.AreEqual(2, attemptCount);
    }

    private sealed class InterruptingFetchEngine : IFetchEngine<int>
    {
        public MakoClient MakoClient => null!;

        public EngineHandle EngineHandle { get; } = new(Guid.NewGuid());

        public int RequestedPages { get; set; }

        public int EnumeratorCount { get; private set; }

        public int MoveNextCount { get; private set; }

        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            EnumeratorCount++;
            return new Enumerator(this);
        }

        private sealed class Enumerator(InterruptingFetchEngine engine) : IAsyncEnumerator<int>
        {
            public int Current { get; private set; }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

            public ValueTask<bool> MoveNextAsync()
            {
                engine.MoveNextCount++;
                switch (engine.MoveNextCount)
                {
                    case 1:
                        Current = 1;
                        return ValueTask.FromResult(true);
                    case 2:
                        return ValueTask.FromResult(false);
                    case 3:
                        Current = 2;
                        return ValueTask.FromResult(true);
                    default:
                        engine.EngineHandle.Complete();
                        return ValueTask.FromResult(false);
                }
            }
        }
    }
}
