// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Controls.Timeline;

public enum TimelineAxisPlacement
{
    Left, Right
}

public static class TimelineAxisPlacementExtension
{
    public static TimelineAxisPlacement Inverse(this TimelineAxisPlacement placement)
    {
        return placement switch
        {
            TimelineAxisPlacement.Left => TimelineAxisPlacement.Right,
            TimelineAxisPlacement.Right => TimelineAxisPlacement.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }
}
