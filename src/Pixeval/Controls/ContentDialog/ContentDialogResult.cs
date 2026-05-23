// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Controls;

/// <summary>
/// Describes how a <see cref="ContentDialog"/> was closed.
/// </summary>
public enum ContentDialogResult
{
    /// <summary>
    /// The dialog closed without an action button result.
    /// </summary>
    None,

    /// <summary>
    /// The primary action button closed the dialog.
    /// </summary>
    Primary,

    /// <summary>
    /// The secondary action button closed the dialog.
    /// </summary>
    Secondary,

    /// <summary>
    /// The close action button closed the dialog.
    /// </summary>
    Close
}
