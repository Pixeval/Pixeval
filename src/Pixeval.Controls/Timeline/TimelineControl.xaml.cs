// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;

namespace Pixeval.Controls.Timeline;

public sealed partial class TimelineControl
{
    [GeneratedDependencyProperty]
    public partial DataTemplate? ItemTemplate { get; set; }

    public TimelineControl() => InitializeComponent();
}
