#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2024 Pixeval.Utilities/Streams.cs
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
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IO;
using System.Linq;

namespace Pixeval.Utilities;

public static class Streams
{
    private const int BlockSizeInBytes = 1024; // 1KB

    private const int LargeBufferMultipleInBytes = 1024 * BlockSizeInBytes; // 1MB

    private const int MaxBufferSizeInBytes = 16 * 1024 * BlockSizeInBytes; // 16MB

    private const int MaximumLargeBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

    private const int MaximumSmallBufferPoolSizeInBytes = 24 * 1024 * BlockSizeInBytes; // 24MB

    private static readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager =
        new(new RecyclableMemoryStreamManager.Options(
            BlockSizeInBytes,
            LargeBufferMultipleInBytes,
            MaxBufferSizeInBytes,
            MaximumSmallBufferPoolSizeInBytes,
            MaximumLargeBufferPoolSizeInBytes));

    public static MemoryStream RentStream() => _recyclableMemoryStreamManager.GetStream();

    public static MemoryStream RentStream(ReadOnlySpan<byte> span) => _recyclableMemoryStreamManager.GetStream(span);

    public static async Task<MemoryStream> CopyToMemoryStreamAsync(this Stream source, bool dispose)
    {
        var s = _recyclableMemoryStreamManager.GetStream();
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
                var s = _recyclableMemoryStreamManager.GetStream();
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
        var zipStream = _recyclableMemoryStreamManager.GetStream();

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
