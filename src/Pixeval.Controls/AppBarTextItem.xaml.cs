// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using FluentIcons.Common;

namespace Pixeval.Controls;

public sealed partial class AppBarTextItem
{
    [GeneratedDependencyProperty]
    public partial string? Text { get; set; }

    [GeneratedDependencyProperty]
    public partial Symbol Symbol { get; set; }

    public AppBarTextItem() => InitializeComponent();
}
