#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ContentFiller.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<double>("DesiredWidth")]
[DependencyProperty<double>("DesiredHeight")]
public sealed partial class ContentFiller : ContentControl
{
    protected override Size MeasureOverride(Size availableSize)
    {
        ((FrameworkElement)Content).Measure(availableSize);
        var r = availableSize;
        if (availableSize.Width is double.PositiveInfinity)
            r.Width = DesiredWidth;
        if (availableSize.Height is double.PositiveInfinity)
            r.Height = DesiredHeight;
        return r;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        ((FrameworkElement)Content).Arrange(new Rect(new Point(), finalSize));
        return finalSize;
    }
}
