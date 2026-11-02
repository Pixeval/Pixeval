// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mako.Global.Exception;
using Mako.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Models.Subscriptions;
using WebApiClientCore.Exceptions;

namespace Pixeval.Tests;

[TestClass]
public sealed class WorkSubscriptionSyncHelperTest
{
    [TestMethod]
    [DataRow(403, false)]
    [DataRow(404, false)]
    [DataRow(403, true)]
    [DataRow(404, true)]
    public async Task PrepareWorks_SkipsUnavailableWorkAndKeepsBothNeighbours(int status, bool makoException)
    {
        var staged = new List<string>();
        var errors = new List<Exception>();
        Exception failure = makoException
            ? new MakoNetworkException("test", false, null, status)
            : new HttpRequestException("test", null, (HttpStatusCode) status);

        foreach (var work in new[] { "A", "B", "C" })
        {
            var prepared = await WorkSubscriptionSyncHelper.TryPrepareWorkAsync(
                () => work is "B" ? Task.FromException<string>(failure) : Task.FromResult(work),
                errors.Add,
                CancellationToken.None);
            if (prepared is not null)
                staged.Add(prepared);
        }

        Assert.AreSequenceEqual(["A", "C"], staged);
        Assert.AreSequenceEqual([failure], errors);

        // Skipping does not mark the work as completed: it can succeed on the next sync.
        var retry = await WorkSubscriptionSyncHelper.TryPrepareWorkAsync(
            () => Task.FromResult("B"), errors.Add, CancellationToken.None);
        Assert.AreEqual("B", retry);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    public async Task PrepareWork_SkipsApiResponseAccessFailure(HttpStatusCode status)
    {
        using var response = new HttpResponseMessage(status);
        var failure = new ApiResponseStatusException(response);
        var errors = new List<Exception>();
        var result = await WorkSubscriptionSyncHelper.TryPrepareWorkAsync(
            () => Task.FromException<string>(failure), errors.Add, CancellationToken.None);

        Assert.IsNull(result);
        Assert.AreSequenceEqual([failure], errors);
    }

    [TestMethod]
    [DataRow(429)]
    [DataRow(500)]
    public async Task PrepareWork_PreservesTransientRequestRetries(int status)
    {
        var attempts = 0;
        var errors = new List<Exception>();
        var result = await WorkSubscriptionSyncHelper.TryPrepareWorkAsync(
            () => FetchEngineRetryHelper.ExecuteAsync(
                _ => ++attempts is 1
                    ? Task.FromException<string>(new HttpRequestException("test", null, (HttpStatusCode) status))
                    : Task.FromResult("A"),
                static _ => TimeSpan.Zero),
            errors.Add,
            CancellationToken.None);

        Assert.AreEqual("A", result);
        Assert.AreEqual(2, attempts);
        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public async Task PrepareWork_PropagatesUnexpectedErrorsAndCancellation()
    {
        Exception[] failures =
        [
            new InvalidOperationException("test"),
            new HttpRequestException("test", null, HttpStatusCode.Unauthorized),
            new HttpRequestException("test", null, HttpStatusCode.TooManyRequests),
            new HttpRequestException("test"),
            new OperationCanceledException()
        ];
        var errors = new List<Exception>();
        foreach (var failure in failures)
        {
            var thrown = await Assert.ThrowsAsync<Exception>(() => WorkSubscriptionSyncHelper.TryPrepareWorkAsync(
                () => Task.FromException<string>(failure), errors.Add, CancellationToken.None));
            Assert.AreSame(failure, thrown);
        }
        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public async Task PrepareWork_DoesNotSkipAccessFailureAfterCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var errors = new List<Exception>();
        _ = await Assert.ThrowsExactlyAsync<HttpRequestException>(() => WorkSubscriptionSyncHelper.TryPrepareWorkAsync<string>(
            () =>
            {
                cancellation.Cancel();
                throw new HttpRequestException("test", null, HttpStatusCode.Forbidden);
            },
            errors.Add,
            cancellation.Token));
        Assert.IsEmpty(errors);

        _ = await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => WorkSubscriptionSyncHelper.TryPrepareWorkAsync(
            () => Task.FromResult("A"), errors.Add, cancellation.Token));
    }
}
