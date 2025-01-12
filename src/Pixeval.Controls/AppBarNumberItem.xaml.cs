// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using FluentIcons.Common;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Title")]
[DependencyProperty<int>("Number")]
[DependencyProperty<Symbol>("Symbol")]
public sealed partial class AppBarNumberItem
{
    public AppBarNumberItem() => InitializeComponent();
}
