using System;
using System.Threading.Tasks;

namespace Pixeval.Controls;

/// <summary>
/// Defers a dialog close operation until asynchronous work is complete.
/// </summary>
public sealed class ContentDialogDeferral
{
    private readonly TaskCompletionSource _completion = new();
    private bool _isCompleted;

    internal Task Task => _completion.Task;

    /// <summary>
    /// Completes the deferred operation.
    /// </summary>
    public void Complete()
    {
        if (_isCompleted)
            throw new InvalidOperationException("The deferral has already been completed.");

        _isCompleted = true;
        _completion.TrySetResult();
    }
}
