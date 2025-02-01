// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Controls;

public sealed partial class DigitalSignalItem
{
    [GeneratedDependencyProperty]
    public partial string? Text { get; set; }

    [GeneratedDependencyProperty]
    public partial Brush? Fill { get; set; }

    public DigitalSignalItem() => InitializeComponent();
}
