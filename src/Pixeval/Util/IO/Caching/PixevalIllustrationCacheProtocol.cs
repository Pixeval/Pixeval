#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2025 Pixeval/PixevalIllustrationCacheProtocol.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pixeval.Caching;
using Pixeval.Utilities.Memory;

namespace Pixeval.Util.IO.Caching;

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
