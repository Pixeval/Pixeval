// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;

namespace Pixeval.Controls;

internal interface IAdaptiveGridLayoutInfo
{
    int Lines { get; }

    int ItemsPerLine { get; }

    event EventHandler<AvaloniaPropertyChangedEventArgs>? PropertyChanged;
}
