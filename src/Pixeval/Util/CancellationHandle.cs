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
                Interlocked.Increment(ref _isCancelled);
            }
        }

        public void Reset()
        {
            if (IsCancelled)
            {
                Interlocked.Decrement(ref _isCancelled);
            }
        }
    }
}