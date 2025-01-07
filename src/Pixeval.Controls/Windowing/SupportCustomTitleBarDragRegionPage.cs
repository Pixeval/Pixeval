// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Windows.Graphics;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class SupportCustomTitleBarDragRegionPage : EnhancedWindowPage
{
    /// <summary>
    /// Informs the bearer to refresh the drag region.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="windowSize"></param>
    /// <param name="scaleFactor"></param>
    /// <param name="titleBarHeight">小于0不会自动设置<see cref="NonClientRegionKind.LeftBorder"/>、<see cref="NonClientRegionKind.RightBorder"/>、<see cref="NonClientRegionKind.Caption"/></param>
    protected virtual void SetTitleBarDragRegion(InputNonClientPointerSource sender, SizeInt32 windowSize, double scaleFactor, out int titleBarHeight)
    {
        titleBarHeight = -1;
    }

    public void RaiseSetTitleBarDragRegion(Window window)
    {
        try
        {
            if (!window.AppWindow.IsVisible || !AppWindowTitleBar.IsCustomizationSupported())
                return;
        }
        catch
        {
            return;
        }
        // UIElement.RasterizationScale 恒为1
        var source = InputNonClientPointerSource.GetForWindowId(window.AppWindow.Id);
        var scaleFactor = XamlRoot.RasterizationScale;
        var size = window.AppWindow.Size;
        // 区域数量为0或1时，下次AppWindow会自动恢复为默认的区域；>=2的时候不会，需要手动清除
        source.ClearRegionRects(NonClientRegionKind.Passthrough);
        SetTitleBarDragRegion(source, size, scaleFactor, out var titleBarHeight);
        window.SetCaptionAndBorder(titleBarHeight, this);
    }

    protected RectInt32 GetScaledRect(RectInt32 rect)
    {
        return rect.GetScaledRectFrom(this);
    }

    protected RectInt32 GetScaledRect(UIElement uiElement)
    {
        return uiElement.GetScaledRectFrom(this);
    }
}
