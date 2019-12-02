using System;
using System.Collections.Generic;
using System.Linq;
using Pzxlane.Objects.Exceptions;

namespace Pzxlane.Objects
{
    public static class Retry
    {
        public static TOut For<T, TOut, TException>(IEnumerable<T> enumerable, Func<T, TOut> mappingFunction, int limit = int.MaxValue)
        {
            var list = enumerable.ToList();
            for (var i = 0; i < limit; i++)
            {
                if (i <= list.Count - 1)
                {
                    try
                    {
                        return mappingFunction(list[i]);
                    }
                    catch (Exception e) when (e is TException)
                    {
                        // ignored
                    }
                }
                else
                {
                    break;
                }
            }
            throw new RetryLimitException($"retry limit: {limit}");
        }
    }
}