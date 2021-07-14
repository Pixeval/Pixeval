using System;
using System.Threading.Tasks;

namespace Pixeval.Util
{
    public static class Functions
    {
        public static void IgnoreException(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // ignore
            }
        }

        public static async Task IgnoreExceptionAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch
            {
                // ignore
            }
        }

        public static T Identity<T>(T o) => o;
    }
}