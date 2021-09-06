#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Result.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Util
{
    [PublicAPI]
    public record Result<T>
    {
        public T GetOrThrow()
        {
            return this switch
            {
                Success (var content) => content,
                Failure (var cause)   => throw cause ?? new Exception("This is an exception thrown by Result.Failure"),
                _                     => throw new Exception("Invalid derived type of Result<T>")
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> OfSuccess(T value)
        {
            return new Success(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> OfFailure(Exception? cause = null)
        {
            return new Failure(cause);
        }

        public Result<R> Bind<R>(Func<T, R> selector)
        {
            return this switch
            {
                Success(var content) => Result<R>.OfSuccess(selector(content)),
                Failure(var cause)   => Result<R>.OfFailure(cause),
                _                    => throw new Exception("Invalid derived type of Result<T>")
            };
        }

        public static Result<R?> Wrap<R>(Result<T> result) where R : class
        {
            return result.Bind(t => t as R);
        }

        [PublicAPI]
        public record Success : Result<T>
        {
            public Success(T value)
            {
                Value = value;
            }

            public T Value { get; }

            public void Deconstruct(out T value)
            {
                value = Value;
            }
        }

        [PublicAPI]
        public record Failure : Result<T>
        {
            public Failure(Exception? cause)
            {
                Cause = cause;
            }

            public Exception? Cause { get; }

            public void Deconstruct([CanBeNull] out Exception? cause)
            {
                cause = Cause;
            }
        }
    }
}