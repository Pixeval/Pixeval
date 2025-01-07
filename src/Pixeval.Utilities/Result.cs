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
            Failure(var cause) => ThrowUtils.Throw<T>(cause ?? new Exception("This is an exception thrown by Result.Failure")),
            _ => ThrowUtils.ArgumentOutOfRange<Result<T>, T>(this, "Invalid derived type of Result<T>")
        };
    }

    public T? UnwrapOrElse(T? @else)
    {
        return this switch
        {
            Success(var content) => content,
            Failure => @else,
            _ => ThrowUtils.ArgumentOutOfRange<Result<T>, T>(this, "Invalid derived type of Result<T>")
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
            _ => ThrowUtils.ArgumentOutOfRange<Result<T>, Result<TNew>>(this, "Invalid derived type of Result<T>")
        };
    }

    public async Task<Result<TNew>> RewrapAsync<TNew>(Func<T, Task<TNew>> selector)
    {
        return this switch
        {
            Success(var content) => Result<TNew>.AsSuccess(await selector(content)),
            Failure(var cause) => Result<TNew>.AsFailure(cause),
            _ => ThrowUtils.ArgumentOutOfRange<Result<T>, Result<TNew>>(this, "Invalid derived type of Result<T>")
        };
    }

    public Result<TNew> Cast<TNew>() where TNew : class
    {
        return Rewrap(t => t as TNew ?? ThrowUtils.InvalidCast<TNew>());
    }

    public record Success(T Value) : Result<T>;

    public record Failure(Exception? Cause) : Result<T>;
}
