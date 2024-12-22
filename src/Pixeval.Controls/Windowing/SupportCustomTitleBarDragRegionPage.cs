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
