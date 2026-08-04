// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Models.Database;
using Pixeval.Models.Subscriptions;

namespace Pixeval.Tests;

[TestClass]
public sealed class WorkSubscriptionSyncRequestQueueTest
{
    [TestMethod]
    public void TryEnqueue_GlobalRequestSupersedesPendingSubscriptions()
    {
        var queue = new WorkSubscriptionSyncRequestQueue();
        var first = CreateSubscriptionRequest(1);
        var second = CreateSubscriptionRequest(2);

        Assert.IsTrue(queue.TryEnqueue(first, null));
        Assert.IsFalse(queue.TryEnqueue(first, null));
        Assert.IsTrue(queue.TryEnqueue(second, null));

        Assert.IsTrue(queue.TryEnqueue(WorkSubscriptionSyncRequest.All.Instance, null));
        Assert.AreEqual(1, queue.Count);
        Assert.IsFalse(queue.TryEnqueue(first, null));
        Assert.IsTrue(queue.TryDequeue(out var request));
        Assert.IsInstanceOfType<WorkSubscriptionSyncRequest.All>(request);
    }

    [TestMethod]
    public void TryEnqueue_SubscriptionDuringActiveGlobalRequestRemainsPending()
    {
        var queue = new WorkSubscriptionSyncRequestQueue();
        var request = CreateSubscriptionRequest(1);
        var activeRequest = WorkSubscriptionSyncRequest.All.Instance;

        Assert.IsTrue(queue.TryEnqueue(request, activeRequest));
        Assert.IsFalse(queue.TryEnqueue(request, activeRequest));
        Assert.IsFalse(queue.TryEnqueue(WorkSubscriptionSyncRequest.All.Instance, activeRequest));
        Assert.AreEqual(1, queue.Count);
    }

    [TestMethod]
    public void TryEnqueue_DeduplicatesActiveSubscriptionByHistoryEntryId()
    {
        var queue = new WorkSubscriptionSyncRequestQueue();
        var activeRequest = CreateSubscriptionRequest(1);

        Assert.IsFalse(queue.TryEnqueue(CreateSubscriptionRequest(1), activeRequest));
        Assert.IsTrue(queue.TryEnqueue(CreateSubscriptionRequest(2), activeRequest));
    }

    [TestMethod]
    public void RemoveSubscription_RemovesOnlyMatchingPendingRequest()
    {
        var queue = new WorkSubscriptionSyncRequestQueue();
        _ = queue.TryEnqueue(CreateSubscriptionRequest(1), null);
        _ = queue.TryEnqueue(CreateSubscriptionRequest(2), null);
        _ = queue.TryEnqueue(CreateSubscriptionRequest(3), null);

        queue.RemoveSubscription(2);

        Assert.AreEqual(2, queue.Count);
        Assert.IsTrue(queue.TryDequeue(out var first));
        Assert.AreEqual(1, Assert.IsInstanceOfType<WorkSubscriptionSyncRequest.Subscription>(first)
            .Entry.HistoryEntryId);
        Assert.IsTrue(queue.TryDequeue(out var second));
        Assert.AreEqual(3, Assert.IsInstanceOfType<WorkSubscriptionSyncRequest.Subscription>(second)
            .Entry.HistoryEntryId);
    }

    [TestMethod]
    public void RemoveSubscription_PreservesPendingGlobalRequest()
    {
        var queue = new WorkSubscriptionSyncRequestQueue();
        _ = queue.TryEnqueue(WorkSubscriptionSyncRequest.All.Instance, null);

        queue.RemoveSubscription(1);

        Assert.AreEqual(1, queue.Count);
        Assert.IsTrue(queue.TryDequeue(out var request));
        Assert.IsInstanceOfType<WorkSubscriptionSyncRequest.All>(request);
    }

    private static WorkSubscriptionSyncRequest.Subscription CreateSubscriptionRequest(int id) =>
        new(new WorkSubscriptionEntry { HistoryEntryId = id });
}
