// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mako.Global.Exception;
using WebApiClientCore.Exceptions;

namespace Pixeval.Models.Subscriptions;

internal static class WorkSubscriptionSyncHelper
{
    public static async Task<T?> TryPrepareWorkAsync<T>(
        Func<Task<T>> prepare,
        Action<Exception> reportUnavailable,
        CancellationToken token) where T : class
    {
        token.ThrowIfCancellationRequested();
        try
        {
            return await prepare().ConfigureAwait(false);
        }
        catch (Exception exception) when (!token.IsCancellationRequested && IsUnavailable(exception))
        {
            // Only work-level access failures can be skipped; retries and cancellation belong to the caller.
            reportUnavailable(exception);
            return null;
        }
    }

    private static bool IsUnavailable(Exception exception) => exception switch
    {
        MakoNetworkException { StatusCode: 403 or 404 } => true,
        ApiResponseStatusException { StatusCode: HttpStatusCode.Forbidden or HttpStatusCode.NotFound } => true,
        HttpRequestException { StatusCode: HttpStatusCode.Forbidden or HttpStatusCode.NotFound } => true,
        _ => false
    };
}
