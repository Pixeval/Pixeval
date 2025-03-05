// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.IO;

namespace Pixeval.Utilities;

public static class Streams
{
    private const int BlockSizeInBytes = 1024; // 1KB

    private const int LargeBufferMultipleInBytes = 1024 * BlockSizeInBytes; // 1MB

    private const int MaxBufferSizeInBytes = 16 * 1024 * BlockSizeInBytes; // 16MB

    private const int MaximumLargeBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

    private const int MaximumSmallBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

    private static readonly RecyclableMemoryStreamManager _RecyclableMemoryStreamManager =
        new(new RecyclableMemoryStreamManager.Options(
            BlockSizeInBytes,
            LargeBufferMultipleInBytes,
            MaxBufferSizeInBytes,
            MaximumSmallBufferPoolSizeInBytes,
            MaximumLargeBufferPoolSizeInBytes));

    public static MemoryStream RentStream() => _RecyclableMemoryStreamManager.GetStream();

    public static MemoryStream RentStream(ReadOnlySpan<byte> span) => _RecyclableMemoryStreamManager.GetStream(span);

    public static async Task<MemoryStream> CopyToMemoryStreamAsync(this Stream source, bool dispose)
    {
        var s = _RecyclableMemoryStreamManager.GetStream();
        await source.CopyToAsync(s);
        s.Position = 0;
        if (dispose)
            await source.DisposeAsync();
        return s;
    }

    public static async Task<IReadOnlyList<MemoryStream>> ReadZipAsync(Stream zipStream, bool dispose)
    {
        if (zipStream is not MemoryStream ms)
        {
            ms = await zipStream.CopyToMemoryStreamAsync(dispose);
            dispose = true;
        }

        try
        {
            var list = new List<MemoryStream>();
            // Dispose ZipArchive会导致ms也被Dispose
            var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            // ZipArchive不支持多线程
            foreach (var entry in archive.Entries)
            {
                await using var stream = entry.Open();
                var s = _RecyclableMemoryStreamManager.GetStream();
                await stream.CopyToAsync(s);
                s.Position = 0;
                list.Add(s);
            }
            if (dispose)
                await ms.DisposeAsync();
            return list;
        }
        catch (InvalidDataException)
        {
            ms.Position = 0;
            return [ms];
        }
    }

    public static async Task<MemoryStream> WriteZipAsync(IReadOnlyList<string> names, IReadOnlyList<Stream> streams, bool dispose)
    {
        var zipStream = _RecyclableMemoryStreamManager.GetStream();

        var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);

        for (var i = 0; i < streams.Count; ++i)
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
}
