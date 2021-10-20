using System;

namespace Pixeval.Util
{
    public static class ThrowHelper
    {
        public static void ThrowException<TException>(params object[] parameters) where TException : Exception
        {
            throw (Exception) Activator.CreateInstance(typeof(TException), parameters)!;
        }

        public static TResult ThrowException<TException, TResult>(params object[] parameters) where TException : Exception
        {
            throw (Exception) Activator.CreateInstance(typeof(TException), parameters)!;
        }

        public static void ThrowIf<TException>(bool condition, params object[] parameters) where TException : Exception
        {
            if (condition)
            {
                ThrowException<TException>(parameters);
            }
        }
    }
}