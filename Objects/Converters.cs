using System;
using System.Threading.Tasks;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Objects
{
    public static class Converters
    {
        public static IConverter<TIn, TOut> From<TIn, TOut>(Type converterType)
        {
            if (converterType.IsImplementOf(typeof(IConverter<TIn, TOut>)))
            {
                return Reflection.NewInstance(converterType) as IConverter<TIn, TOut>;
            }
            throw new TypeMismatchException($"{nameof(converterType)} is not a derived type of {nameof(IConverter<TIn, TOut>)}");
        }

        public static IAsyncConverter<TIn, TOut> FromAsync<TIn, TOut>(Type converterType)
        {
            if (converterType.IsImplementOf(typeof(IAsyncConverter<TIn, TOut>)))
            {
                return Reflection.NewInstance(converterType) as IAsyncConverter<TIn, TOut>;
            }
            throw new TypeMismatchException($"{nameof(converterType)} is not a derived type of {nameof(IConverter<TIn, TOut>)}");
        }
    }

    public interface IAsyncConverter<in TIn, TOut> : IConverter<TIn, Task<TOut>>
    {
    }

    public interface IConverter<in TIn, out TOut>
    {
        TOut Convert(TIn input);
    }
}