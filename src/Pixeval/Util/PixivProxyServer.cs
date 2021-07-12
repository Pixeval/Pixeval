using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Pixeval.Util
{
    public class ProxyException : Exception
    {
        public ProxyException()
        {
        }

        protected ProxyException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ProxyException([CanBeNull] string? message) : base(message)
        {
        }

        public ProxyException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class PixivProxyServer : IDisposable
    {
        private readonly X509Certificate2? _certificate;
        private readonly TcpListener? _tcpListener;

        private static Task<IPAddress[]> GetTargetIp(string host)
        {
            return !host.Contains("pixiv") ? Dns.GetHostAddressesAsync(host) : Task.FromResult(new[]
            {
                IPAddress.Parse("210.140.131.219"),
                IPAddress.Parse("210.140.131.223"),
                IPAddress.Parse("210.140.131.226")
            });
        }

        private PixivProxyServer(X509Certificate2 certificate, TcpListener tcpListener)
        {
            _certificate = certificate;
            _tcpListener = tcpListener;
            _tcpListener.Start();
            StartAccept();
        }

        public static PixivProxyServer Create(IPAddress ip, int port, X509Certificate2 certificate)
        {
            return new(certificate, new TcpListener(ip, port));
        }

        private async void StartAccept()
        {
            while (_tcpListener?.Server is not null)
            {
                using var client = await _tcpListener!.AcceptTcpClientAsync();
                var clientStream = client.GetStream();
                var content = await new StreamReader(clientStream).ReadLineAsync();
                if (content?.StartsWith("CONNECT") is not true)
                {
                    continue;
                }

                await using var writer = new StreamWriter(clientStream);
                await writer.WriteLineAsync("HTTP/1.1 200 Connection established");
                await writer.WriteLineAsync($"Timestamp: {DateTime.Now}");
                await writer.WriteLineAsync("Proxy-agent: Pixeval");
                await writer.WriteLineAsync();
                await writer.FlushAsync();
                var clientSsl = new SslStream(clientStream, false);
                await clientSsl.AuthenticateAsServerAsync(_certificate!, false, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13, false);
                var host = Regex.Match(content, "CONNECT (?<host>.+)\\:\\d+").Groups["host"].Value;
                SslStream? serverSsl = null;
                foreach (var ip in await GetTargetIp(host))
                {
                    try
                    {
                        serverSsl = await CreateConnection(ip.ToString());
                        break;
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (serverSsl is null)
                {
                    throw new ProxyException(string.Format(MiscResources.ProxyServerConnectToTargetHostFailedFormatted, host));
                }

                await Functions.IgnoreExceptionAsync(async () =>
                {
                    await clientSsl.CopyToAsync(serverSsl);
                    await serverSsl.CopyToAsync(clientSsl);
                });
                serverSsl.Close();
            }
        }

        private static async Task<SslStream> CreateConnection(string ip)
        {
            var client = new TcpClient();
            await client.ConnectAsync(ip, 443);
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

        public static int NegotiatePort()
        {
            var unavailable = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Select(t => t.LocalEndPoint.Port).ToArray();
            var rd = new Random();
            var proxyPort = rd.Next(3000, 65536);
            while (Array.BinarySearch(unavailable, proxyPort) >= 0)
            {
                proxyPort = rd.Next(3000, 65536);
            }

            return proxyPort;
        }

        public void Dispose()
        {
            _certificate?.Dispose();
            _tcpListener?.Stop();
        }
    }
}