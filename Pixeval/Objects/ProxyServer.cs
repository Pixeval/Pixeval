#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

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
using Pixeval.Data.Web.Delegation;

namespace Pixeval.Objects
{
    internal class ProxyServer : IDisposable
    {
        private readonly X509Certificate2 certificate;
        private readonly TcpListener tcpListener;

        private static string GetTargetIp(string host)
        {
            return host.Contains("pixiv.net") ? PixivApiDnsResolver.Instance.Lookup()[0].ToString() : Dns.GetHostAddresses(host)[0].ToString();
        }

        /// <summary>
        ///     Create an <see cref="ProxyServer" /> with specified host, port, target and certificate
        /// </summary>
        /// <param name="host">proxy server host</param>
        /// <param name="port">proxy server port to listen</param>
        /// <param name="x509Certificate2">server certificate</param>
        private ProxyServer(string host, int port, X509Certificate2 x509Certificate2)
        {
            certificate = x509Certificate2;
            tcpListener = new TcpListener(IPAddress.Parse(host), port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, tcpListener);
        }

        public void Dispose()
        {
            certificate?.Dispose();
            tcpListener.Stop();
        }

        public static ProxyServer Create(string host, int port, X509Certificate2 x509Certificate2)
        {
            return new ProxyServer(host, port, x509Certificate2);
        }

        private async void AcceptTcpClientCallback(IAsyncResult result)
        {
            // stupid hacks with nested try-catch, never mind
            try
            {
                var listener = (TcpListener)result.AsyncState;
                if (listener != null)
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
                        await clientSsl.AuthenticateAsServerAsync(certificate, false, SslProtocols.Tls | SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11, false);
                        // create an HTTP connection to the target IP
                        var host = Regex.Match(content, "CONNECT (?<host>.+)\\:\\d+").Groups["host"].Value;
                        var serverSsl = await CreateConnection(GetTargetIp(host), host);

                        var request = Task.Run(() =>
                        {
                            try
                            {
                                clientSsl.CopyTo(serverSsl);
                            }
                            catch
                            {
                                // ignore
                            }
                        });
                        var response = Task.Run(() =>
                        {
                            try
                            {
                                serverSsl.CopyTo(clientSsl);
                            }
                            catch
                            {
                                // ignore
                            }
                        });
                        Task.WaitAny(request, response);
                        serverSsl.Close();
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private static async Task<SslStream> CreateConnection(string ip, string host)
        {
            var client = new TcpClient();
            await client.ConnectAsync(ip, 443);
            var netStream = client.GetStream();
            var sslStream = new SslStream(netStream, false, (sender, certificate, chain, errors) => true);
            try
            {
                await sslStream.AuthenticateAsClientAsync(host.Contains("pixiv.net") ? "" : host);
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
    }
}