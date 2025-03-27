// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mako.Net;
using Pixeval.Utilities;

namespace Pixeval.Util;

public partial class PixivAuthenticationProxyServer : IDisposable
{
    private readonly X509Certificate2? _certificate;
    private readonly TcpListener? _tcpListener;

    private PixivAuthenticationProxyServer(X509Certificate2 certificate, TcpListener tcpListener)
    {
        _certificate = certificate;
        _tcpListener = tcpListener;
        _tcpListener.Start();
        _ = _tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, _tcpListener);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _certificate?.Dispose();
        _tcpListener?.Stop();
    }

    public static PixivAuthenticationProxyServer Create(IPAddress ip, int port, X509Certificate2 certificate)
    {
        return new PixivAuthenticationProxyServer(certificate, new TcpListener(ip, port));
    }

    private async void AcceptTcpClientCallback(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is TcpListener listener)
            {
                using var client = listener.EndAcceptTcpClient(result);
                _ = listener.BeginAcceptTcpClient(AcceptTcpClientCallback, listener);
                using (client)
                {
                    var clientStream = client.GetStream();
                    var content = await new StreamReader(clientStream).ReadLineAsync();
                    // content starts with "CONNECT" means it's trying to establish an HTTPS connection
                    if (content is null || !content.StartsWith("CONNECT"))
                    {
                        return;
                    }

                    await using var writer = new StreamWriter(clientStream);
                    await writer.WriteLineAsync("HTTP/1.1 200 Connection established");
                    await writer.WriteLineAsync($"Timestamp: {DateTime.Now}");
                    await writer.WriteLineAsync("Proxy-agent: Pixeval");
                    await writer.WriteLineAsync();
                    await writer.FlushAsync();
                    var clientSsl = new SslStream(clientStream, false);
                    // use specify certificate to establish the HTTPS connection
                    await clientSsl.AuthenticateAsServerAsync(_certificate!, false, SslProtocols.None, false);
                    // create an HTTP connection to the target IP
                    var host = HostRegex().Match(content).Groups["host"].Value;
                    var serverSsl = await CreateConnection(await MakoHttpOptions.GetAddressesAsync(host, CancellationToken.None));
                    var request = Functions.IgnoreExceptionAsync(() => clientSsl.CopyToAsync(serverSsl));
                    var response = Functions.IgnoreExceptionAsync(() => serverSsl.CopyToAsync(clientSsl));
                    _ = await Task.WhenAny(request, response);
                    serverSsl.Close();
                }
            }
        }
        catch
        {
            // ignore
        }
    }

    private static async Task<SslStream> CreateConnection(IPAddress[] ipAddresses)
    {
        var client = new TcpClient();
        await client.ConnectAsync(ipAddresses, 443);
        var netStream = client.GetStream();
        var sslStream = new SslStream(netStream, false, (_, _, _, _) => true);
        try
        {
            await sslStream.AuthenticateAsClientAsync(string.Empty);
            return sslStream;
        }
        catch
        {
            await sslStream.DisposeAsync();
            throw;
        }
    }

    [GeneratedRegex(@"CONNECT (?<host>.+)\:\d+")]
    private static partial Regex HostRegex();
}
