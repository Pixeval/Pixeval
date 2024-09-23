#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IOHelper.cs
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Pixeval.Util.IO;

public static partial class IoHelper
{
    public static async Task<string> Sha1Async(this Stream stream)
    {
        using var sha1 = SHA1.Create();
        var result = await sha1.ComputeHashAsync(stream);
        stream.Position = 0; // reset the stream
        return result.Select(b => b.ToString("X2")).Aggregate((acc, str) => acc + str);
    }

    public static string GetInvalidPathChars { get; } = @"*?""|" + new string(Path.GetInvalidPathChars());

    public static string GetInvalidNameChars { get; } = @"\/*:?""|" + new string(Path.GetInvalidPathChars());

    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(GetInvalidPathChars.Aggregate(path, (s, c) => s.Replace(c.ToString(), ""))).TrimEnd('.');
    }

    public static string NormalizePathSegment(string path)
    {
        return GetInvalidNameChars.Aggregate(path, (s, c) => s.Replace(c.ToString(), "")).TrimEnd('.');
    }

    public static void CreateParentDirectories(string fullPath)
    {
        var directory = Path.GetDirectoryName(fullPath);
        _ = Directory.CreateDirectory(directory!);
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
        _ = await dataWriter.StoreAsync();
        _ = dataWriter.DetachStream();
        return stream;
    }

    public static async Task<IImageFormat> DetectImageFormat(this IRandomAccessStream randomAccessStream)
    {
        await using var stream = randomAccessStream.AsStream();
        return await Image.DetectFormatAsync(stream);
    }

    public static async Task<string> ToBase64StringAsync(this IRandomAccessStream randomAccessStream)
    {
        var array = ArrayPool<byte>.Shared.Rent((int)randomAccessStream.Size);
        var buffer = await randomAccessStream.ReadAsync(array.AsBuffer(), (uint)randomAccessStream.Size, InputStreamOptions.None);
        ArrayPool<byte>.Shared.Return(array);
        return Convert.ToBase64String(buffer.ToArray());
    }

    public static async Task<string> GenerateBase64UrlForImageAsync(this IRandomAccessStream randomAccessStream)
    {
        var base64Str = await randomAccessStream.ToBase64StringAsync();
        var format = await randomAccessStream.DetectImageFormat();
        return $"data:image/{format?.Name.ToLower()},{base64Str}";
    }

    public static Task<HttpResponseMessage> PostFormAsync(this HttpClient httpClient, string url, params (string? Key, string? Value)[] parameters)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(parameters.Select(tuple => new KeyValuePair<string?, string?>(tuple.Key, tuple.Value)))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
                }
            }
        };
        return httpClient.SendAsync(httpRequestMessage);
    }

    public static async Task<MemoryStream> CopyToMemoryStreamAsync(this FileStream source, bool dispose)
    {
        var s = _recyclableMemoryStreamManager.GetStream();
        await source.CopyToAsync(s);
        s.Position = 0;
        if (dispose)
            await source.DisposeAsync();
        return s;
    }

    public static async Task<MemoryStream[]> ReadZipAsync(Stream zipStream, bool dispose)
    {
        Stream s;
        if (zipStream is FileStream fs)
        {
            s = await fs.CopyToMemoryStreamAsync(dispose);
            dispose = true;
        }
        else
        {
            s = zipStream;
        }

        using var archive = new ZipArchive(s, ZipArchiveMode.Read);
        // return the result of Select directly will cause the enumeration to be delayed
        // which will lead the program into ObjectDisposedException since the archive object
        // will be disposed after the execution of ReadZipArchiveEntries
        // So we must consume the archive.Entries.Select right here, prevent it from escaping
        // to the outside of the stackframe
        MemoryStream[] result = await Task.WhenAll(
            archive.Entries.Select(async entry =>
            {
                await using var stream = entry.Open();
                var ms = _recyclableMemoryStreamManager.GetStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;
                return ms;
            }));
        if (dispose)
            await s.DisposeAsync();
        return result;
    }

    public static async Task<MemoryStream> WriteZipAsync(IReadOnlyList<string> names, IReadOnlyList<Stream> streams, bool dispose)
    {
        var zipStream = _recyclableMemoryStreamManager.GetStream();

        var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);

        for (var i = 0; i < streams.Count; i++)
        {
            // ReSharper disable once AccessToDisposedClosure
            var entry = zipArchive.CreateEntry(names[i]);
            await using var entryStream = entry.Open();
            await streams[i].CopyToAsync(entryStream);
            if (dispose)
                await streams[i].DisposeAsync();
        }

        zipArchive.Dispose();
        // see-also https://stackoverflow.com/questions/47707862/ziparchive-gives-unexpected-end-of-data-corrupted-error/47707973
        // 在flush前释放ZipArchive
        zipStream.Position = 0;
        await zipStream.FlushAsync();

        return zipStream;
    }

    public static void DeleteEmptyFolder(string? path)
    {
        if (Directory.Exists(path))
            if (!Directory.EnumerateFileSystemEntries(path).Any())
                Directory.Delete(path);
    }

    public static FileStream OpenAsyncRead(string path, int bufferSize = 4096)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
    }

    public static FileStream OpenAsyncWrite(string path, int bufferSize = 4096)
    {
        return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
    }

    public static FileStream OpenAsyncRead(this FileInfo info, int bufferSize = 4096)
    {
        return info.Open(new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            BufferSize = bufferSize,
            Options = FileOptions.Asynchronous
        });
    }

    public static FileStream OpenAsyncWrite(this FileInfo info, int bufferSize = 4096)
    {
        return info.Open(new FileStreamOptions
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
            Share = FileShare.None,
            BufferSize = bufferSize,
            Options = FileOptions.Asynchronous
        });
    }
}
