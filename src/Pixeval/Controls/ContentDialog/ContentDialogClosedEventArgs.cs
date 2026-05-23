// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Controls;

/// <summary>
/// Provides data for a dialog closed event.
/// </summary>
/// <param name="result">The result returned by the closed dialog.</param>
public sealed class ContentDialogClosedEventArgs(ContentDialogResult result)
{
    /// <summary>
    /// Gets the result returned by the closed dialog.
    /// </summary>
    public ContentDialogResult Result { get; } = result;
}
