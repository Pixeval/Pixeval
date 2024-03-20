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
using Pixeval.Download.Models;
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

    public static async Task<MemoryStream[]> ReadZipArchiveEntriesAsync(Stream zipStream, bool dispose)
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

    public static async Task DeleteIllustrationTaskAsync(IllustrationDownloadTaskBase task)
    {
        try
        {
            if (task is MangaDownloadTask)
                await (await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(task.Destination))).DeleteAsync(StorageDeleteOption.Default);
            else
                await (await StorageFile.GetFileFromPathAsync(task.Destination)).DeleteAsync(StorageDeleteOption.Default);
        }
        catch
        {
            // ignored
        }
    }
}
