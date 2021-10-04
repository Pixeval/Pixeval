using Windows.Foundation;

namespace Pixeval.Util
{
    public static class AsyncHelper
    {
        public static void Discard<T>(this IAsyncOperation<T> _)
        {
            // nop
        }
    }
}