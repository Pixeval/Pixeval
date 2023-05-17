#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ISupportCustomTitleBarDragRegion.cs
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

using System.Threading.Tasks;
using Windows.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval;

public interface ISupportCustomTitleBarDragRegion
{
    /// <summary>
    /// Informs the bearer to refresh the drag region, notice that both <param name="titleBar"></param> and <param name="dragRegions"></param>
    /// are nullable, the are optional to be used in the implementation of this function. BUT THIS IS A IMPLEMENTATION DETAIL.
    /// </summary>
    /// <param name="titleBar"></param>
    /// <param name="dragRegions"></param>
    /// <returns></returns>
    Task<RectInt32[]> SetTitleBarDragRegionAsync(FrameworkElement? titleBar, ColumnDefinition[]? dragRegions);
}