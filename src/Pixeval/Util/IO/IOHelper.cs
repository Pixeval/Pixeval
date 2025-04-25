// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;

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

    public static FileStream CreateAsyncWrite(string path, int bufferSize = 4096)
    {
        return new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, true);
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
