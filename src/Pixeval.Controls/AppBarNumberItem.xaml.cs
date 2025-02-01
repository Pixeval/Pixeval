// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using FluentIcons.Common;

namespace Pixeval.Controls;

public sealed partial class AppBarNumberItem
{
    [GeneratedDependencyProperty]
    public partial string? Title { get; set; }

    [GeneratedDependencyProperty]
    public partial int Number { get; set; }

    [GeneratedDependencyProperty]
    public partial Symbol Symbol { get; set; }

    public AppBarNumberItem() => InitializeComponent();
}
