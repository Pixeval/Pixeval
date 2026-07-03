// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.AppManagement;

namespace Pixeval.Utilities.GitHub;

public static class GitHubDirectHttpClientFactory
{
    private static readonly TimeSpan _ConnectAttemptTimeout = TimeSpan.FromSeconds(5);

    public static HttpClient Create(NetworkSettingsGroup networkSettings)
    {
        var handler = new SocketsHttpHandler
        {
            UseProxy = true,
            Proxy = GitHubDirectProxy.Create(networkSettings),
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(20),
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            ConnectCallback = (context, token) => ConnectAsync(networkSettings, context, token)
        };

        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromSeconds(60),
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
        };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(nameof(Pixeval), AppInfo.AppVersion.CurrentVersionShortText));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        return client;
    }

    private static async ValueTask<Stream> ConnectAsync(
        NetworkSettingsGroup networkSettings,
        SocketsHttpConnectionContext context,
        CancellationToken token)
    {
        var endpoint = context.DnsEndPoint;
        if (networkSettings.EnablePixivDomainFronting &&
            networkSettings.EnableGitHubDirectConnection &&
            GitHubHttpOptions.TryGetConfiguredAddresses(networkSettings, endpoint.Host, out var addresses))
        {
            return await ConnectToAddressesAsync(endpoint.Host, endpoint.Port, addresses, token).ConfigureAwait(false);
        }

        return await ConnectToDnsEndPointAsync(endpoint, token).ConfigureAwait(false);
    }

    private static async ValueTask<Stream> ConnectToDnsEndPointAsync(DnsEndPoint endpoint, CancellationToken token)
    {
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        try
        {
            await socket.ConnectAsync(endpoint, token).ConfigureAwait(false);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    private static async ValueTask<Stream> ConnectToAddressesAsync(
        string host,
        int port,
        IReadOnlyList<IPAddress> addresses,
        CancellationToken token)
    {
        var failures = new List<Exception>();

        foreach (var address in addresses.Distinct())
        {
            token.ThrowIfCancellationRequested();

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(_ConnectAttemptTimeout);

            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, port), timeout.Token).ConfigureAwait(false);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (OperationCanceledException exception) when (!token.IsCancellationRequested)
            {
                socket.Dispose();
                failures.Add(new TimeoutException($"Timed out connecting to {host}:{port} through {address}.", exception));
            }
            catch (Exception exception) when (exception is SocketException or IOException)
            {
                socket.Dispose();
                failures.Add(new IOException($"Failed connecting to {host}:{port} through {address}.", exception));
            }
        }

        throw new IOException(
            $"Could not connect to {host}:{port}. Tried: {string.Join(", ", addresses.Select(static address => address.ToString()))}.",
            new AggregateException(failures));
    }
}
