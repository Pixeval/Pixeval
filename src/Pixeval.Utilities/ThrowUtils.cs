// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

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
    public static TResult Throw<TException, TResult>(TException e) where TException : Exception
        => throw e;

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

    /// <exception cref="NotSupportedException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void NotSupported(string? message = null)
        => throw new NotSupportedException(message);

    /// <exception cref="ArgumentException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Argument(string? message = null, Exception? innerException = null)
        => throw new ArgumentException(message, innerException);

    /// <exception cref="ArgumentOutOfRangeException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentOutOfRange<T>(T? actualValue, string? message = null, [CallerArgumentExpression(nameof(actualValue))] string? paraName = null)
        => throw new ArgumentOutOfRangeException(paraName, actualValue, message);

    /// <exception cref="ArgumentOutOfRangeException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ArgumentOutOfRange<T, TReturn>(T? actualValue, string? message = null, [CallerArgumentExpression(nameof(actualValue))] string? paraName = null)
        => throw new ArgumentOutOfRangeException(paraName, actualValue, message);

    /// <exception cref="MacroParseException"/>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void MacroParse(string? message = null)
        => throw new MacroParseException(message);

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

