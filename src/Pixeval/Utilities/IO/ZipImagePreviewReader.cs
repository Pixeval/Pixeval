// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

namespace Pixeval.Utilities.IO;

/// <summary>
/// Reads complete local ZIP entries without waiting for the central directory (ugoira).
/// ZIP64, encryption and entries with trailing size descriptors fall back to the full loader.
/// </summary>
internal sealed class ZipImagePreviewReader : IDisposable
{
    private const int MaximumEntryBytes = 8 * 1024 * 1024;
    private long _offset;
    private MemoryStream? _frame;
    private bool _unsupported;

    public Stream? ReadLatestFrame(Stream source)
    {
        if (_unsupported)
            return null;

        var position = source.Position;
        Span<byte> header = stackalloc byte[30];
        long frameOffset = 0;
        var compressedSize = 0;
        var expandedSize = 0;
        var compression = 0;
        try
        {
            while (source.Length - _offset >= header.Length)
            {
                source.Position = _offset;
                source.ReadExactly(header);
                if (BinaryPrimitives.ReadUInt32LittleEndian(header) is not 0x04034b50)
                    break;
                var flags = BinaryPrimitives.ReadUInt16LittleEndian(header[6..]);
                var method = BinaryPrimitives.ReadUInt16LittleEndian(header[8..]);
                var compressed = BinaryPrimitives.ReadUInt32LittleEndian(header[18..]);
                var expanded = BinaryPrimitives.ReadUInt32LittleEndian(header[22..]);
                if ((flags & 9) is not 0 || method is not 0 and not 8
                    || compressed > MaximumEntryBytes || expanded > MaximumEntryBytes)
                {
                    _unsupported = true;
                    break;
                }

                var start = _offset + header.Length
                    + BinaryPrimitives.ReadUInt16LittleEndian(header[26..])
                    + BinaryPrimitives.ReadUInt16LittleEndian(header[28..]);
                var end = start + compressed;
                if (source.Length < end)
                    break;
                _offset = end;
                if (expanded is 0)
                    continue;
                // Skip intermediate frames if several arrived since the previous preview.
                frameOffset = start;
                compressedSize = (int) compressed;
                expandedSize = (int) expanded;
                compression = method;
            }

            if (expandedSize is 0)
                return null;

            _frame?.Dispose();
            _frame = null;
            source.Position = frameOffset;
            var encoded = new byte[compressedSize];
            source.ReadExactly(encoded);
            if (compression is 0)
                _frame = new MemoryStream(encoded, false);
            else
            {
                using var compressed = new MemoryStream(encoded, false);
                using var inflater = new DeflateStream(compressed, CompressionMode.Decompress);
                var expanded = new byte[expandedSize];
                inflater.ReadExactly(expanded);
                if (inflater.ReadByte() is not -1)
                    throw new InvalidDataException("ZIP preview exceeds the declared frame size.");
                _frame = new MemoryStream(expanded, false);
            }

            return _frame;
        }
        finally
        {
            source.Position = position;
        }
    }

    public void Dispose()
    {
        _frame?.Dispose();
        _frame = null;
    }
}
