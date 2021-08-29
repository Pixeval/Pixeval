using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Util.Generic
{
    public interface ILocalizedBox<T, L> where L : ILocalizedBox<T, L>
    {
        public T Value { get; }

        public string LocalizedString { get; }

        static abstract IEnumerable<L> AvailableOptions();
    }

    public static class LocalizedBoxHelper
    {
        public static TLocalizedBox Of<TOption, TLocalizedBox>(TOption option)
            where TLocalizedBox : ILocalizedBox<TOption, TLocalizedBox>
        {
            return TLocalizedBox.AvailableOptions().First(t => t.Value!.Equals(option));
        }
    }
}