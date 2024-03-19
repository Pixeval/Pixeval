#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/FontWeightsOption.cs
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

using Microsoft.UI.Text;
using Pixeval.Attributes;

namespace Pixeval.Options;

public enum FontWeightsOption
{
    /// <summary>
    /// <see cref="FontWeights.Thin"/>
    /// </summary>
    /// <remarks>100</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Thin))]
    Thin,

    /// <summary>
    /// <see cref="FontWeights.ExtraLight"/>
    /// </summary>
    /// <remarks>200</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ExtraLight))]
    ExtraLight,

    /// <summary>
    /// <see cref="FontWeights.Light"/>
    /// </summary>
    /// <remarks>300</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Light))]
    Light,

    /// <summary>
    /// <see cref="FontWeights.SemiLight"/>
    /// </summary>
    /// <remarks>350</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.SemiLight))]
    SemiLight,

    /// <summary>
    /// <see cref="FontWeights.Normal"/>
    /// </summary>
    /// <remarks>400</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Normal))]
    Normal,

    /// <summary>
    /// <see cref="FontWeights.Medium"/>
    /// </summary>
    /// <remarks>500</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Medium))]
    Medium,

    /// <summary>
    /// <see cref="FontWeights.SemiBold"/>
    /// </summary>
    /// <remarks>600</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.SemiBold))]
    SemiBold,

    /// <summary>
    /// <see cref="FontWeights.Bold"/>
    /// </summary>
    /// <remarks>700</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Bold))]
    Bold,

    /// <summary>
    /// <see cref="FontWeights.ExtraBold"/>
    /// </summary>
    /// <remarks>800</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ExtraBold))]
    ExtraBold,

    /// <summary>
    /// <see cref="FontWeights.Black"/>
    /// </summary>
    /// <remarks>900</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Black))]
    Black,

    /// <summary>
    /// <see cref="FontWeights.ExtraBlack"/>
    /// </summary>
    /// <remarks>350</remarks>
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.ExtraBlack))]
    ExtraBlack
}
