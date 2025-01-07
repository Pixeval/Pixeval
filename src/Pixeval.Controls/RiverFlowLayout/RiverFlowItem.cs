// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Windows.Foundation;
using Microsoft.UI.Xaml;

namespace Pixeval.Controls;

internal class RiverFlowItem(int index)
{
    public int Index { get; } = index;

    public Size? DesiredSize { get; internal set; }

    public Size? Measure { get; internal set; }

    public Point? Position { get; internal set; }

    public UIElement? Element { get; internal set; }
}
