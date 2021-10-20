using System;
using System.Threading;

namespace Pixeval.Util
{
    /// <summary>
    /// A re-entrant cancellation helper that prevents the default behaviors of <see cref="CancellationToken"/>, which is,
    /// throws an <see cref="OperationCanceledException"/>
    /// </summary>
    public class CancellationHandle
    {
        private Action? _onCancellation;
        private int _isCancelled;

        public bool IsCancelled => _isCancelled == 1;

        public void Cancel()
        {
            if (!IsCancelled)
            {
                _onCancellation?.Invoke();
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

        public void Register(Action action)
        {
            _onCancellation += action;
        }

        public static CancellationHandle FromCancellationToken(CancellationToken token)
        {
            var cancellationHandle = new CancellationHandle();
            token.Register(() => cancellationHandle.Cancel());
            return cancellationHandle;
        }
    }
}