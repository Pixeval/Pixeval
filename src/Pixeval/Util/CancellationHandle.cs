using System;
using System.Threading;

namespace Pixeval.Util
{
    /// <summary>
    /// A cancellation helper that prevents the default behaviors of <see cref="CancellationToken"/>, which is,
    /// throws an <see cref="OperationCanceledException"/>
    /// </summary>
    public class CancellationHandle
    {
        private int _isCancelled;

        public bool IsCancelled => _isCancelled == 1;

        public void Cancel()
        {
            if (!IsCancelled)
            {
                int current;
                do
                {
                    current = _isCancelled;
                } while (current != Interlocked.CompareExchange(ref _isCancelled, 1, 0));
            }
        }

        public void Reset()
        {
            if (IsCancelled)
            {
                int current;
                do
                {
                    current = _isCancelled;
                } while (current != Interlocked.CompareExchange(ref _isCancelled, 0, 1));
            }
        }
    }
}