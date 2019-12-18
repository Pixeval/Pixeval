using System;

namespace Pixeval.Objects
{
    internal static class Functions
    {
        public static T Apply<T>(this T receiver, Action<T> action)
        {
            action(receiver);
            return receiver;
        }
    }
}