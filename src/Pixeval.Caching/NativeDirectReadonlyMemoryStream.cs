#region Copyright (c) Pixeval/Pixeval.Caching
// GPL v3 License
// 
// Pixeval/Pixeval.Caching
// Copyright (c) 2025 Pixeval.Caching/NativeDirectReadonlyMemoryStream.cs
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

namespace Pixeval.Caching;

// This stream requires no disposal
public class NativeDirectReadonlyStream(Memory<byte> span) : Stream
{
    public override void Flush()
    {
        // we don't have any write operation here so flush is not needed
        throw new NotSupportedException("Not supported by NativeDirectReadonlyStream");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        span.Span.Slice((int) Position, count).CopyTo(buffer.AsSpan(offset, count));
        Position += count;
        return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var loc1 = origin switch
        {
            SeekOrigin.Begin => 0,
            SeekOrigin.Current => Position,
            SeekOrigin.End => Length,
            _ => throw new ArgumentException("Invalid seek origin")
        };
        return SeekCore(offset, loc1);


        long SeekCore(long offs, long loc)
        {
            if (offs > int.MaxValue - loc)
                throw new ArgumentOutOfRangeException(nameof(offs));
            var num = loc + (int) offs;
            if (loc + offs < 0L || num < 0L)
                throw new IOException("Seek before begin");
            Position = num;
            return Position;
        }
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("Not supported by NativeDirectReadonlyStream");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Not supported by NativeDirectReadonlyStream");
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => span.Length;

    public override long Position { get; set; }
}
