using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage;
using CommunityToolkit.WinUI.Helpers;
using Mako.Util;

namespace Pixeval.Util
{
    public static partial class IOHelper
    {
        public static async Task<string> CalculateChecksumAsync<T>(string fullnameOfFile) where T : HashAlgorithm, new()
        {
            return await (await File.ReadAllBytesAsync(fullnameOfFile)).HashAsync<T>();
        }
        
        public static async Task ClearDirectoryAsync(this StorageFolder dir)
        {
            await Task.WhenAll((await dir.GetItemsAsync()).Select(f => f.DeleteAsync().AsTask()));
        }

        public static async Task WriteBytesAsync(this Stream stream, byte[] bytes)
        {
            await stream.WriteAsync(bytes);
        }

        public static async Task WriteBytesAsync(this StorageStreamTransaction storageStreamTransaction, byte[] bytes)
        {
            await storageStreamTransaction.Stream.WriteAsync(CryptographicBuffer.CreateFromByteArray(bytes));
        }

        public static Task WriteStringAsync(this StorageFile storageFile, string str)
        {
            return storageFile.WriteBytesAsync(str.GetBytes());
        }

        public static async Task WriteBytesAsync(this StorageFile storageFile, byte[] bytes)
        {
            using var storageStreamTransaction = await storageFile.OpenTransactedWriteAsync(StorageOpenOptions.AllowOnlyReaders);
            await storageStreamTransaction.WriteBytesAsync(bytes);
            await storageStreamTransaction.CommitAsync();
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

        public static async Task<string> ReadStringAsync(this StorageFile storageFile, Encoding? encoding = null)
        {
            return (await storageFile.ReadBytesAsync()).GetString(encoding);
        }

        public static Task<HttpResponseMessage> PostFormAsync(this HttpClient httpClient, string url, params (string key, string value)[] parameters)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(parameters.Select(tuple => new KeyValuePair<string?, string?>(tuple.key, tuple.value)))
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
                    }
                }
            };
            return httpClient.SendAsync(httpRequestMessage);
        }
    }
}