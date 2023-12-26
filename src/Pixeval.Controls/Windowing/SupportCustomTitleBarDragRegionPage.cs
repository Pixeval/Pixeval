#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/ISupportCustomTitleBarDragRegion.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using Windows.Foundation;
using Windows.Graphics;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;

namespace Pixeval.Controls;

public class SupportCustomTitleBarDragRegionPage : EnhancedWindowPage
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

    public void RaiseSetTitleBarDragRegion()
    {
        // UIElement.RasterizationScale 恒为1
        var source = InputNonClientPointerSource.GetForWindowId(Window.AppWindow.Id);
        var scaleFactor = XamlRoot.RasterizationScale;
        var size = Window.AppWindow.Size;
        // 区域数量为0或1时，下次AppWindow会自动恢复为默认的区域；>=2的时候不会，需要手动清除
        source.ClearRegionRects(NonClientRegionKind.Passthrough);
        SetTitleBarDragRegion(source, size, scaleFactor, out var titleBarHeight);
        if (titleBarHeight >= 0)
        {
            // 三大金刚下面的区域
            const int borderThickness = 5;
            source.SetRegionRects(NonClientRegionKind.LeftBorder, [GetScaledRect(new RectInt32(0, 0, borderThickness, titleBarHeight))]);
            source.SetRegionRects(NonClientRegionKind.RightBorder, [GetScaledRect(new RectInt32(size.Width, 0, borderThickness, titleBarHeight))]);
            source.SetRegionRects(NonClientRegionKind.Caption, [GetScaledRect(new RectInt32(0, 0, size.Width, titleBarHeight))]);
        }
    }

    protected RectInt32 GetScaledRect(RectInt32 rect)
    {
        var scaleFactor = XamlRoot.RasterizationScale;
        return new RectInt32((int)(rect.X * scaleFactor), (int)(rect.Y * scaleFactor), (int)(rect.Width * scaleFactor), (int)(rect.Height * scaleFactor));
    }

    protected RectInt32 FromControl(UIElement uiElement)
    {
        var pos = uiElement.TransformToVisual(null).TransformPoint(new Point(0, 0));
        var rect = new RectInt32((int)pos.X, (int)pos.Y, (int)uiElement.ActualSize.X, (int)uiElement.ActualSize.Y);
        return GetScaledRect(rect);
    }
}
