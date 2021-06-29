using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Util
{
    [PublicAPI]
    public record Result<T>
    {
        [PublicAPI]
        public record Success : Result<T>
        {
            public T Value { get; }

            public void Deconstruct(out T value)
            {
                value = Value;
            }

            public Success(T value)
            {
                Value = value;
            }
        }

        [PublicAPI]
        public record Failure : Result<T>
        {
            public Exception? Cause { get; }

            public void Deconstruct([CanBeNull] out Exception? cause)
            {
                cause = Cause;
            }

            public Failure(Exception? cause)
            {
                Cause = cause;
            }
        }

        public T GetOrThrow()
        {
            return this switch
            {
                Success (var content) => content,
                Failure (var cause)   => throw cause ?? new Exception("This is an exception thrown by Result.Failure"),
                _                     => throw new ArgumentException("Result", "Result", null)
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> OfSuccess(T value) => new Success(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> OfFailure(Exception? cause = null) => new Failure(cause);

        public static Result<R?> Wrap<R>(Result<T> result) where R : class
        {
            return result switch
            {
                Success (var content) => Result<R?>.OfSuccess(content as R),
                Failure (var cause)   => Result<R?>.OfFailure(cause),
                _                     => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }
    }
}