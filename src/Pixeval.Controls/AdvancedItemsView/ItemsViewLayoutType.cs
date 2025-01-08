// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

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
