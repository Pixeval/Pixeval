#region Copyright (c) Pixeval/Pixeval.Controls

// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/ItemsViewLayoutType.cs
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

using Pixeval.Attributes;

namespace Pixeval.Controls;

[LocalizationMetadata(typeof(AdvancedItemsViewResources))]
public enum ItemsViewLayoutType
{
    [LocalizedResource(typeof(AdvancedItemsViewResources), nameof(AdvancedItemsViewResources.LinedFlow))]
    LinedFlow,

    [LocalizedResource(typeof(AdvancedItemsViewResources), nameof(AdvancedItemsViewResources.Grid))]
    Grid,

    VerticalUniformStack,

    HorizontalUniformStack,

    VerticalStack,

    HorizontalStack,

    Staggered
}
