#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IOHelper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using JetBrains.Annotations;
using Pixeval.Utilities;

namespace Pixeval.Util.IO;

public static partial class IOHelper
{
    public static async Task<string> Sha1Async(this IRandomAccessStream randomAccessStream)
    {
        using var sha1 = SHA1.Create();
        var result = await sha1.ComputeHashAsync(randomAccessStream.AsStreamForRead());
        randomAccessStream.Seek(0); // reset the stream
        return result.Select(b => b.ToString("x2")).Aggregate((acc, str) => acc + str);
    }

    public static async Task CreateAndWriteToFileAsync(IRandomAccessStream contentStream, string path)
    {
        CreateParentDirectories(path);
        await using var stream = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        contentStream.Seek(0);
        await contentStream.AsStreamForRead().CopyToAsync(stream);
    }

    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(Path.GetInvalidPathChars().Aggregate(path, (s, c) => s.Replace(c.ToString(), string.Empty)));
    }

    public static string NormalizePathSegment(string path)
    {
        return Path.GetInvalidFileNameChars().Aggregate(path, (s, c) => s.Replace(c.ToString(), string.Empty));
    }

    public static void CreateParentDirectories(string fullPath)
    {
        var directory = Path.GetDirectoryName(fullPath);
        Directory.CreateDirectory(directory!);
    }

    public static async Task ClearDirectoryAsync(this StorageFolder dir)
    {
        await Task.WhenAll((await dir.GetItemsAsync()).Select(f => f.DeleteAsync().AsTask()));
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

    [ContractAnnotation("notnull => notnull")]
    public static async Task<byte[]?> ReadBytesAsync(this StorageFile? file)
    {
        if (file == null)
        {
            return null;
        }

        using IRandomAccessStream stream = await file.OpenReadAsync();
        using var reader = new DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync((uint) stream.Size);
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

    public static async Task SaveToFileAsync(this IRandomAccessStream stream, StorageFile file)
    {
        stream.Seek(0);
        using var dataReader = new DataReader(stream);
        await dataReader.LoadAsync((uint) stream.Size);
        await FileIO.WriteBufferAsync(file, dataReader.ReadBuffer((uint) stream.Size));
        dataReader.DetachStream();
    }

    public static async Task DeleteAsync(string path)
    {
        try
        {
            await using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose)) { }
        }
        catch
        {
            // ignored
        }
    }
}