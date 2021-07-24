using System;
using System.Collections.Generic;

namespace Pixeval.Util
{
    public static class Enumerates
    {
        public static void AddSorted<T>(this IList<T> list, T item, Func<T?, T?, int> comparer)
        {
            var i = 0;
            while (i < list.Count && comparer(list[i], item) < 0)
            {
                i++;
            }

            list.Insert(i, item);
        }
    }
}