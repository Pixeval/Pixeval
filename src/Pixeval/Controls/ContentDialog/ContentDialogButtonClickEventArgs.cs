using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Controls;

/// <summary>
/// Provides data for a dialog button click event.
/// </summary>
/// <param name="button">The button that was clicked.</param>
/// <param name="result">The result that will be returned if the dialog closes.</param>
public sealed class ContentDialogButtonClickEventArgs(
    ContentDialogButton button,
    ContentDialogResult result)
{
    private readonly List<ContentDialogDeferral> _deferrals = [];

    /// <summary>
    /// Gets the button that was clicked.
    /// </summary>
    public ContentDialogButton Button { get; } = button;

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
    /// <returns>A deferral for asynchronous click handling.</returns>
    public ContentDialogDeferral GetDeferral()
    {
        var deferral = new ContentDialogDeferral();
        _deferrals.Add(deferral);
        return deferral;
    }

    internal Task WaitForDeferralsAsync() => Task.WhenAll(_deferrals.ConvertAll(static d => d.Task));
}
