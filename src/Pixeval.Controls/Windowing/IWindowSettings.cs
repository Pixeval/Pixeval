// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace Pixeval.Controls.Windowing;

public interface IWindowSettings
{
    public BackdropType Backdrop { get; }

    public ElementTheme Theme { get; }
}
