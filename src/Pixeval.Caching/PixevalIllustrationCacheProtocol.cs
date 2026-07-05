// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pixeval.Caching;
using Pixeval.Utilities.Memory;

namespace Pixeval.Utilities.IO.Caching;

[StructLayout(LayoutKind.Sequential)]
public record struct PixevalIllustrationCacheHeader(int BinarySize);

public class PixevalIllustrationCacheKey(string key) : IEquatable<PixevalIllustrationCacheKey>
{
    public string Key { get; init; } = key;

    public int BinarySize { get; set; }

    public PixevalIllustrationCacheKey(string key, int binarySize) : this(key)
    {
        BinarySize = binarySize;
    }

    public bool Equals(PixevalIllustrationCacheKey? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((PixevalIllustrationCacheKey) obj);
    }

    public override int GetHashCode() => Key.GetHashCode();
}

public class PixevalIllustrationCacheProtocol : ICacheProtocol<PixevalIllustrationCacheKey, PixevalIllustrationCacheHeader>
{
    public PixevalIllustrationCacheHeader GetHeader(PixevalIllustrationCacheKey key)
    {
        return new PixevalIllustrationCacheHeader(key.BinarySize);
    }

    public Span<byte> SerializeHeader(PixevalIllustrationCacheHeader header)
    {
        return MemoryHelper.ConvertToBytes(header);
    }

    public PixevalIllustrationCacheHeader DeserializeHeader(Span<byte> span)
    {
        unsafe
        {
            var ptr = (int*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            return new PixevalIllustrationCacheHeader(*ptr);
        }
    }

    public int GetDataLength(PixevalIllustrationCacheHeader header)
    {
        return header.BinarySize;
    }
}
