// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Text;
using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
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
