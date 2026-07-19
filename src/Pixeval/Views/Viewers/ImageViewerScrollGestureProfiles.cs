// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SmoothScroll.Avalonia.Interaction;

namespace Pixeval.Views.Viewers;

internal static class ImageViewerScrollGestureProfiles
{
    public static ScrollGestureBindings Paging => CreateOverride(ScrollGestureAction.Zoom);

    public static ScrollGestureBindings Horizontal => CreateOverride(ScrollGestureAction.HorizontalScroll);

    public static ScrollGestureBindings Vertical => CreateImageViewerDefault();

    private static ScrollGestureBindings CreateOverride(ScrollGestureAction wheelAction)
    {
        var bindings = CreateImageViewerDefault();
        bindings[new ScrollGesture(ScrollInputGesture.MouseWheel)] = wheelAction;
        return bindings;
    }

    private static ScrollGestureBindings CreateImageViewerDefault()
    {
        var bindings = ScrollGestureBindings.CreateDefault();
        bindings[new ScrollGesture(ScrollInputGesture.MouseLeftDrag)] = ScrollGestureAction.Pan;
        return bindings;
    }
}
