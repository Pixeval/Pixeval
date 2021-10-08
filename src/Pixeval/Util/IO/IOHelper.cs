﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Pixeval.Utilities;

namespace Pixeval.Util.IO
{
    public static partial class IOHelper
    {
        public static async Task ClearDirectoryAsync(this StorageFolder dir)
        {
            await Task.WhenAll((await dir.GetItemsAsync()).Select(f => f.DeleteAsync().AsTask()));
        }

        public static IRandomAccessStream AsRandomAccessStream(this IBuffer buffer)
        {
            return buffer.AsStream().AsRandomAccessStream();
        }

        public static async Task<IRandomAccessStream> GetRandomAccessStreamFromByteArrayAsync(byte[] byteArray)
        {
            var stream = new InMemoryRandomAccessStream();
            using var dataWriter = new DataWriter(stream.GetOutputStreamAt(0));
            dataWriter.WriteBytes(byteArray);
            await dataWriter.StoreAsync();
            dataWriter.DetachStream();
            return stream;
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

        public static IAsyncAction WriteStringAsync(this StorageFile storageFile, string str)
        {
            return storageFile.WriteBytesAsync(str.GetBytes());
        }

        public static IAsyncAction WriteBytesAsync(this StorageFile storageFile, byte[] bytes)
        {
            return FileIO.WriteBytesAsync(storageFile, bytes);
        }

        public static async Task<StorageFile> GetOrCreateFileAsync(this StorageFolder folder, string itemName)
        {
            return await folder.TryGetItemAsync(itemName) is StorageFile file ? file : await folder.CreateFileAsync(itemName, CreationCollisionOption.ReplaceExisting);
        }

        public static async Task<StorageFolder> GetOrCreateFolderAsync(this StorageFolder folder, string folderName)
        {
            return await folder.TryGetItemAsync(folderName) is StorageFolder f ? f : await folder.CreateFolderAsync(folderName, CreationCollisionOption.ReplaceExisting);
        }

        public static async Task<string?> ReadStringAsync(this StorageFile storageFile, Encoding? encoding = null)
        {
            return (await storageFile.ReadBytesAsync())?.GetString(encoding);
        }

        public static async Task<byte[]?> ReadBytesAsync(this StorageFile? file)
        {
            if (file == null)
            {
                return null;
            }

            using IRandomAccessStream stream = await file.OpenReadAsync();
            using var reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            var bytes = new byte[stream.Size];
            reader.ReadBytes(bytes);
            return bytes;
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

        public static async Task SaveToFile(this IRandomAccessStream stream, StorageFile file)
        {
            stream.Seek(0);
            using var dataReader = new DataReader(stream);
            await dataReader.LoadAsync((uint) stream.Size);
            await FileIO.WriteBufferAsync(file, dataReader.ReadBuffer((uint) stream.Size));
            dataReader.DetachStream();
        }
    }
}