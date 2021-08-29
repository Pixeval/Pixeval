using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pixeval.Util.Generic
{
    public static class Enumerates
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }

        public static T[] ArrayOf<T>(params T[] t) => t;

        public static IEnumerable<T> EnumerableOf<T>(params T[] t) => ArrayOf(t);
    }
}