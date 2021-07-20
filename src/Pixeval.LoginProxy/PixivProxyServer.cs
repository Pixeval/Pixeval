using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pixeval.LoginProxy
{
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
            _tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, _tcpListener);
        }

        public static PixivProxyServer Create(IPAddress ip, int port, X509Certificate2 certificate)
        {
            return new(certificate, new TcpListener(ip, port));
        }

        private async void AcceptTcpClientCallback(IAsyncResult result)
        {
            try
            {
                if (result.AsyncState is TcpListener listener)
                {
                    using var client = listener.EndAcceptTcpClient(result);
                    listener.BeginAcceptTcpClient(AcceptTcpClientCallback, listener);
                    using (client)
                    {
                        var clientStream = client.GetStream();
                        var content = await new StreamReader(clientStream).ReadLineAsync();
                        // content starts with "CONNECT" means it's trying to establish an HTTPS connection
                        if (content == null || !content.StartsWith("CONNECT")) return;
                        await using var writer = new StreamWriter(clientStream);
                        await writer.WriteLineAsync("HTTP/1.1 200 Connection established");
                        await writer.WriteLineAsync($"Timestamp: {DateTime.Now}");
                        await writer.WriteLineAsync("Proxy-agent: Pixeval");
                        await writer.WriteLineAsync();
                        await writer.FlushAsync();
                        var clientSsl = new SslStream(clientStream, false);
                        // use specify certificate to establish the HTTPS connection
                        await clientSsl.AuthenticateAsServerAsync(_certificate!, false, SslProtocols.Tls | SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11, false);
                        // create an HTTP connection to the target IP
                        var host = Regex.Match(content, "CONNECT (?<host>.+)\\:\\d+").Groups["host"].Value;
                        var serverSsl = await CreateConnection(await GetTargetIp(host));
                        var request = Functions.IgnoreExceptionAsync(async () => await clientSsl.CopyToAsync(serverSsl));
                        var response = Functions.IgnoreExceptionAsync(async () => await serverSsl.CopyToAsync(clientSsl));
                        await Task.WhenAny(request, response);
                        serverSsl.Close();
                    }
                }
            }
            catch (Exception e)
            {
                if (e is PixivWebLoginException pixivWebLoginException)
                {
                    throw pixivWebLoginException;
                }
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