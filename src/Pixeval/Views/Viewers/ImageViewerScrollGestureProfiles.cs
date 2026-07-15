// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Input;
using SmoothScroll.Avalonia.Interaction;

namespace Pixeval.Views.Viewers;

internal static class ImageViewerScrollGestureProfiles
{
    public static ScrollGestureBindings Paging { get; } = CreateOverride(ScrollGestureAction.Zoom);

    public static ScrollGestureBindings Horizontal { get; } = CreateOverride(ScrollGestureAction.HorizontalScroll);

    public static ScrollGestureBindings Vertical { get; } = ScrollGestureBindings.CreateDefault();

    private static ScrollGestureBindings CreateOverride(ScrollGestureAction wheelAction)
    {
        var bindings = ScrollGestureBindings.CreateDefault();
        bindings[new ScrollGesture(ScrollInputGesture.MouseWheel)] = wheelAction;
        return bindings;
    }
}
