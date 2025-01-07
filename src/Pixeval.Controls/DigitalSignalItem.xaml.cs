// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Media;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Text")]
[DependencyProperty<Brush>("Fill")]
public sealed partial class DigitalSignalItem
{
    public DigitalSignalItem() => InitializeComponent();
}
