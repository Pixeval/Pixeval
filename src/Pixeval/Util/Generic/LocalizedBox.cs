using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Util.Generic
{
    public interface ILocalizedBox<out T>
    {
        public T Value { get; }

        public string LocalizedString { get; }
    }

    public static class LocalizedBoxHelper
    {
        public static TLocalizedBox Of<TOption, TLocalizedBox>(this IEnumerable<TLocalizedBox> source, TOption option)
            where TLocalizedBox : ILocalizedBox<TOption>
        {
            return source.First(t => t.Value!.Equals(option));
        }
    }
}