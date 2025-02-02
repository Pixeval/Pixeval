// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using FluentIcons.Common;

namespace Pixeval.Controls;

public sealed partial class IconText
{
    [GeneratedDependencyProperty]
    public partial Symbol Symbol { get; set; }

    [GeneratedDependencyProperty]
    public partial string? Text { get; set; }

    public IconText() => InitializeComponent();
}
