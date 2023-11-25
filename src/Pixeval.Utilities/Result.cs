#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2023 Pixeval.Utilities/Result.cs
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
using System.Threading.Tasks;

namespace Pixeval.Utilities;

public record Result<T>
{
    public T UnwrapOrThrow()
    {
        return this switch
        {
            Success(var content) => content,
            Failure(var cause) => throw cause ?? new Exception("This is an exception thrown by Result.Failure"),
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }

    public T? UnwrapOrElse(T? @else)
    {
        return this switch
        {
            Success(var content) => content,
            Failure => @else,
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> AsSuccess(T value)
    {
        return new Success(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> AsFailure(Exception? cause = null)
    {
        return new Failure(cause);
    }

    public Result<TNew> Rewrap<TNew>(Func<T, TNew> selector)
    {
        return this switch
        {
            Success(var content) => Result<TNew>.AsSuccess(selector(content)),
            Failure(var cause) => Result<TNew>.AsFailure(cause),
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }

    public async Task<Result<TNew>> RewrapAsync<TNew>(Func<T, Task<TNew>> selector)
    {
        return this switch
        {
            Success(var content) => Result<TNew>.AsSuccess(await selector(content)),
            Failure(var cause) => Result<TNew>.AsFailure(cause),
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }

    public record Success(T Value) : Result<T>;

    public record Failure(Exception? Cause) : Result<T>;
}

public static class ResultHelper
{
    public static Result<TNew?> Cast<TOld, TNew>(this Result<TOld> result) where TNew : class
    {
        return result.Rewrap(t => t as TNew);
    }
}
