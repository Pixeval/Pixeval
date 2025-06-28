// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;

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

    public static string InvalidPathChars { get; } = @"*?""|" + new string(Path.GetInvalidPathChars());

    public static string InvalidNameChars { get; } = @"\/*:?""|" + new string(Path.GetInvalidPathChars());

    public static string InvalidNameCharsInMacro { get; } = @"<>\/*:?""|" + new string(Path.GetInvalidPathChars());

    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(InvalidPathChars.Aggregate(path, (s, c) => s.Replace(c.ToString(), ""))).TrimEnd('.');
    }

    public static string NormalizePathSegmentInMacro(string path)
    {
        return InvalidNameCharsInMacro.Aggregate(path, (s, c) => s.Replace(c.ToString(), "")).TrimEnd('.');
    }

    public static string NormalizePathSegment(string path)
    {
        return InvalidNameChars.Aggregate(path, (s, c) => s.Replace(c.ToString(), "")).TrimEnd('.');
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
}
