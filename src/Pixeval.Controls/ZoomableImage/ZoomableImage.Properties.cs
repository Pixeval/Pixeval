// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using CommunityToolkit.WinUI;

namespace Pixeval.Controls;

public partial class ZoomableImage
{
    [GeneratedDependencyProperty]
    public partial object? Source { get; set; }

    [GeneratedDependencyProperty]
    public partial IReadOnlyList<int>? MsIntervals { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool IsPlaying { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 0)]
    public partial int ImageRotationDegree { get; set; }

    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool ImageIsMirrored { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 1f)]
    public partial float ImageScale { get; set; }

    [GeneratedDependencyProperty]
    public partial ZoomableImageMode Mode { get; set; }

    [GeneratedDependencyProperty(DefaultValue = ZoomableImageMode.Fit)]
    public partial ZoomableImageMode InitMode { get; set; }

    [GeneratedDependencyProperty(DefaultValue = ZoomableImagePosition.AbsoluteCenter)]
    public partial ZoomableImagePosition InitPosition { get; set; }
}
