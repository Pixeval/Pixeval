// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mako;
using Mako.Engine;
using Mako.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Collections;
using Pixeval.ViewModels;

namespace Pixeval.Tests;

[TestClass]
public sealed class IncrementalSourceRetryTest
{
    [TestMethod]
    public async Task InterruptedPageStopsAndResumesTheSameEnumerator()
    {
        using var client = new MakoClient(new(), NullLogger.Instance);
        using var provider = client.Provider;
        var engine = new InterruptedEngine(client);
        using var source = new IncrementalSource<Illustration, Illustration>(engine, static (entry, _) => entry);
        using var collection = new IncrementalLoadingCollection<Illustration>(source);

        Assert.AreEqual(1, await collection.LoadMoreItemsAsync(0));
        Assert.AreEqual(2, engine.MoveNextCount);
        Assert.IsTrue(collection.IsInterrupted);
        Assert.IsTrue(collection.HasMoreItems);

        Assert.AreEqual(1, await collection.LoadMoreItemsAsync(0));
        Assert.AreEqual(4, engine.MoveNextCount);
        Assert.AreEqual(1, engine.EnumeratorCount);
        Assert.AreSequenceEqual(new long[] { 1, 2 }, new long[] { collection[0].Id, collection[1].Id });
        Assert.IsFalse(collection.IsInterrupted);
        Assert.IsFalse(collection.HasMoreItems);
    }

    private sealed class InterruptedEngine(MakoClient client) : IFetchEngine<Illustration>
    {
        public MakoClient MakoClient => client;
        public EngineHandle EngineHandle { get; } = new(Guid.NewGuid());
        public int RequestedPages { get; set; }
        public int EnumeratorCount { get; private set; }
        public int MoveNextCount { get; private set; }

        public IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            EnumeratorCount++;
            return new Enumerator(this);
        }

        private sealed class Enumerator(InterruptedEngine engine) : IAsyncEnumerator<Illustration>
        {
            public Illustration Current { get; private set; } = null!;
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
            public ValueTask<bool> MoveNextAsync()
            {
                switch (++engine.MoveNextCount)
                {
                    case 1:
                        Current = Illustration.CreateDefault() with { Id = 1 };
                        return ValueTask.FromResult(true);
                    case 2:
                        return ValueTask.FromResult(false);
                    case 3:
                        Current = Illustration.CreateDefault() with { Id = 2 };
                        return ValueTask.FromResult(true);
                    default:
                        engine.EngineHandle.Complete();
                        return ValueTask.FromResult(false);
                }
            }
        }
    }
}
