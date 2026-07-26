// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum IllustrationViewerAutoPlayScope
{
    [LocalizedResource(Symbol.Image, EnumResources.IllustrationViewerAutoPlayScope.CurrentWork)]
    CurrentWork,

    [LocalizedResource(Symbol.ImageMultiple, EnumResources.IllustrationViewerAutoPlayScope.AllWorks)]
    AllWorks
}
