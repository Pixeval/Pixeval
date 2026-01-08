// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;

namespace Pixeval.Controls;

[LocalizationMetadata(typeof(AdvancedItemsViewResources))]
public enum ItemsViewLayoutType
{
    [LocalizedResource(nameof(AdvancedItemsViewResources.LinedFlow))]
    LinedFlow,

    [LocalizedResource(nameof(AdvancedItemsViewResources.Grid))]
    Grid,

    VerticalUniformStack,

    HorizontalUniformStack,

    VerticalStack,

    HorizontalStack,

    Staggered
}
