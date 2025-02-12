#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2025 Pixeval.Utilities/MemoryHelper.cs
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
using System.IO;
using System.Numerics;

namespace Pixeval.Utilities.Memory;

public static class MemoryHelper
{
    public static T RoundToNearestMultipleOf<T>(T number, T align)
        where T :
        IBinaryInteger<T>,
        IBitwiseOperators<T, T, T>,
        IAdditionOperators<T, T, T>,
        ISubtractionOperators<T, T, T>
    {
        return number + (~number + T.One & align - T.One);
    }

    public static unsafe Span<byte> ConvertToBytes<T>(T value) where T : unmanaged
    {
        var pointer = (byte*) &value;

        Span<byte> bytes = new byte[sizeof(T)];
        for (var i = 0; i < sizeof(T); i++)
        {
            bytes[i] = pointer[i];
        }

        return bytes;
    }

    public static Memory<T> AsMemory<T>(this Span<T> span) where T : unmanaged
    {
        return new UnmanagedMemoryManager<T>(span).Memory;
    }

    public static Span<byte> ReadEnd(this Stream stream)
    {
        if (stream is MemoryStream s)
        {
            return s.ToArray();
        }

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        _ = ms.Seek(0, SeekOrigin.Begin);
        return ms.ToArray();
    }
}
