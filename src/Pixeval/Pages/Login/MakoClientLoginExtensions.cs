// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mako;

namespace Pixeval.Pages.Login;

internal static class MakoClientLoginExtensions
{
    public static async Task<SslStream> CreateConnectionAsync(this MakoClient makoClient, string host, CancellationToken cancellationToken = default)
    {
        var client = new TcpClient();
        try
        {
            var ipAddresses = await makoClient.Configuration.LookupAsync(host).ConfigureAwait(false);
            await client.ConnectAsync(ipAddresses, 443, cancellationToken).ConfigureAwait(false);
            var sslStream = new SslStream(client.GetStream(), false, (_, _, _, _) => true);
            try
            {
                await sslStream.AuthenticateAsClientAsync("").ConfigureAwait(false);
                return sslStream;
            }
            catch
            {
                await sslStream.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }
}
