using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Controls;

/// <summary>
/// Provides data for a dialog closing event.
/// </summary>
/// <param name="result">The result that will be returned if the dialog closes.</param>
public sealed class ContentDialogClosingEventArgs(ContentDialogResult result)
{
    private readonly List<ContentDialogDeferral> _deferrals = [];

    /// <summary>
    /// Gets the result that will be returned if the dialog closes.
    /// </summary>
    public ContentDialogResult Result { get; } = result;

    /// <summary>
    /// Gets or sets whether the dialog close should be canceled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets a deferral that delays the dialog close until <see cref="ContentDialogDeferral.Complete"/> is called.
    /// </summary>
    /// <returns>A deferral for asynchronous closing work.</returns>
    public ContentDialogDeferral GetDeferral()
    {
        var deferral = new ContentDialogDeferral();
        _deferrals.Add(deferral);
        return deferral;
    }

    internal Task WaitForDeferralsAsync() => Task.WhenAll(_deferrals.ConvertAll(static d => d.Task));
}
