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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Pixeval.Utilities;

public static class ThrowUtils
{
    public static void ThrowException<TException>(params object[] parameters) where TException : Exception
    {
        throw (Exception)Activator.CreateInstance(typeof(TException), parameters)!;
    }

    public static TResult ThrowException<TException, TResult>(params object[] parameters) where TException : Exception
    {
        throw (Exception)Activator.CreateInstance(typeof(TException), parameters)!;
    }

    public static void ThrowIf<TException>(bool condition, params object[] parameters) where TException : Exception
    {
        if (condition)
        {
            ThrowException<TException>(parameters);
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InvalidOperation(string? message = null)
    {
        throw new InvalidOperationException(message);
    }

    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ArgumentOutOfRange<T, TReturn>(T? actualValue, string? message = null, [CallerArgumentExpression(nameof(actualValue))] string? paraName = null)
        => throw new ArgumentOutOfRangeException(paraName, actualValue, message);

    /// <exception cref="FormatException"></exception>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn Format<TReturn>(string? message = null)
        => throw new FormatException(message);
}
