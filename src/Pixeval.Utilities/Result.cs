// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

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
            Failure(var cause) => throw(cause ?? new Exception("This is an exception thrown by Result.Failure")),
            _ => throw new ArgumentOutOfRangeException("this", "Invalid derived type of Result<T>")
        };
    }

    public T? UnwrapOrElse(T? @else)
    {
        return this switch
        {
            Success(var content) => content,
            Failure => @else,
            _ => throw new ArgumentOutOfRangeException("this", "Invalid derived type of Result<T>")
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
            _ => throw new ArgumentOutOfRangeException("this", "Invalid derived type of Result<T>")
        };
    }

    public async Task<Result<TNew>> RewrapAsync<TNew>(Func<T, Task<TNew>> selector)
    {
        return this switch
        {
            Success(var content) => Result<TNew>.AsSuccess(await selector(content)),
            Failure(var cause) => Result<TNew>.AsFailure(cause),
            _ => throw new ArgumentOutOfRangeException("this", "Invalid derived type of Result<T>")
        };
    }

    public Result<TNew> Cast<TNew>() where TNew : class
    {
        return Rewrap(t => t as TNew ?? throw new InvalidCastException());
    }

    public record Success(T Value) : Result<T>;

    public record Failure(Exception? Cause) : Result<T>;
}

public static class ResultExtension
{
    public static Result<R> ViewAs<T, R>(this Result<T>.Failure err)
    {
        return Result<R>.AsFailure(err.Cause);
    }
}
