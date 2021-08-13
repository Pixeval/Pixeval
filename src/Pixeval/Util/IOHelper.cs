using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using CommunityToolkit.WinUI.Helpers;
using ImageMagick;
using Mako.Net.Response;
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

        public static IRandomAccessStream AsRandomAccessStream(this IBuffer buffer)
        {
            return buffer.AsStream().AsRandomAccessStream();
        }

        public static async Task<ImageFormat> DetectImageFormat(this IRandomAccessStream randomAccessStream)
        {
            await using var stream = randomAccessStream.AsStream();
            using var image = Image.FromStream(stream);
            return image.RawFormat;
        }

        public static async Task<string> ToBase64StringAsync(this IRandomAccessStream randomAccessStream)
        {
            var array = ArrayPool<byte>.Shared.Rent((int) randomAccessStream.Size);
            var buffer = await randomAccessStream.ReadAsync(array.AsBuffer(), (uint) randomAccessStream.Size, InputStreamOptions.None);
            ArrayPool<byte>.Shared.Return(array);
            return Convert.ToBase64String(buffer.ToArray());
        }

        public static async Task<string> GenerateBase64UrlForImageAsync(this IRandomAccessStream randomAccessStream)
        {
            var base64Str = await randomAccessStream.ToBase64StringAsync();
            var format = await randomAccessStream.DetectImageFormat();
            return $"data:image/{format.ToString().ToLower()},{base64Str}";
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

        public static async Task<(string filename, Stream content)[]> ReadZipArchiveEntries(Stream zipStream)
        {
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
            // Remarks:
            // return the result of Select directly will cause the enumeration to be delayed
            // which will lead the program into ObjectDisposedException since the archive object
            // will be disposed after the execution of ReadZipArchiveEntries
            // So we must consume the archive.Entries.Select right here, prevent it from escaping
            // to the outside of the stackframe
            return await Task.WhenAll(archive.Entries.Select(async entry =>
            {
                await using var stream = entry.Open();
                var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return (entry.Name, (Stream) ms);
            }));
        }

        public static async Task<Result<IRandomAccessStream>> GetGifStreamResultAsync(Stream zipStream, UgoiraMetadataResponse ugoiraMetadataResponse)
        {
            var entryStreams = await ReadZipArchiveEntries(zipStream);
            var ms = new MemoryStream();
            using var collection = new MagickImageCollection();
            if (ugoiraMetadataResponse.UgoiraMetadataInfo?.Frames is { } fs)
            {
                foreach (var frame in fs)
                {
                    var (_, stream) = entryStreams.FirstOrDefault(e => e.filename == frame.File);
                    await using (stream)
                    {
                        var image = new MagickImage(stream)
                        {
                            AnimationDelay = (int) frame.Delay / 10 // the frame.Delay is based on milliseconds where the AnimationDelay is based on one-hundredth seconds
                        };
                        collection.Add(image);
                    }
                }

                var settings = new QuantizeSettings {Colors = 256};
                collection.Quantize(settings);
                collection.Optimize();
                await collection.WriteAsync(ms, MagickFormat.Gif);
                ms.Seek(0, SeekOrigin.Begin);
                return Result<IRandomAccessStream>.OfSuccess(ms.AsRandomAccessStream());
            }

            return Result<IRandomAccessStream>.OfFailure(new ArgumentNullException(nameof(ugoiraMetadataResponse), @"ugoiraMetadataResponse.UgoiraMetadataInfo.Frames is null"));
        }

        public static async Task SaveToFile(this IRandomAccessStream stream, StorageFile file)
        {
            stream.Seek(0);
            using var dataReader = new DataReader(stream);
            await dataReader.LoadAsync((uint) stream.Size);
            await FileIO.WriteBufferAsync(file, dataReader.ReadBuffer((uint) stream.Size));
        }
    }
}