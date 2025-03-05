// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using Windows.Storage;
using Windows.Storage.Streams;

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
        var array = ArrayPool<byte>.Shared.Rent((int) randomAccessStream.Size);
        var buffer = await randomAccessStream.ReadAsync(array.AsBuffer(), (uint) randomAccessStream.Size, InputStreamOptions.None);
        ArrayPool<byte>.Shared.Return(array);
        return Convert.ToBase64String(buffer.ToArray());
    }

    public static async Task<string> GenerateBase64UrlForImageAsync(this IRandomAccessStream randomAccessStream)
    {
        var base64Str = await randomAccessStream.ToBase64StringAsync();
        var format = await randomAccessStream.DetectImageFormat();
        return $"data:image/{format?.Name.ToLower()},{base64Str}";
    }

    // todo 简化为PostJsonAsync
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
