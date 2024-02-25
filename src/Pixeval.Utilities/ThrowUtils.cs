#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ThrowHelper.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Pixeval.Download.MacroParser;

namespace Pixeval.Utilities;

public static class ThrowUtils
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn Throw<TReturn>(Exception exception) => throw exception;

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Throw(Exception exception) => throw exception;

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TResult Throw<TException, TResult>(params object[] parameters) where TException : Exception
        => throw ((Exception)Activator.CreateInstance(typeof(TException), parameters)!);

    /// <exception cref="InvalidOperationException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    /// <exception cref="InvalidOperationException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn InvalidOperation<TReturn>(string? message = null)
        => throw new InvalidOperationException(message);

    /// <exception cref="InvalidCastException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn InvalidCast<TReturn>(string? message = null)
        => throw new InvalidCastException(message);

    /// <exception cref="NotSupportedException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn NotSupported<TReturn>(string? message = null)
        => throw new NotSupportedException(message);

    /// <exception cref="ArgumentException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Argument(string? message = null, Exception? innerException = null)
        => throw new ArgumentException(message, innerException);

    /// <exception cref="ArgumentOutOfRangeException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ArgumentOutOfRange<T, TReturn>(T? actualValue, string? message = null, [CallerArgumentExpression(nameof(actualValue))] string? paraName = null)
        => throw new ArgumentOutOfRangeException(paraName, actualValue, message);

    /// <exception cref="FormatException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn Format<TReturn>(string? message = null)
        => throw new FormatException(message);

    /// <exception cref="UriFormatException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn UriFormat<TReturn>(string? message = null)
        => throw new UriFormatException(message);

    /// <exception cref="KeyNotFoundException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn KeyNotFound<TReturn>(string? message = null)
        => throw new KeyNotFoundException(message);

    /// <exception cref="MacroParseException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn MacroParse<TReturn>(string? message = null)
        => throw new MacroParseException(message);

    /// <exception cref="JsonException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn Json<TReturn>(string? message = null)
        => throw new JsonException(message);
}

