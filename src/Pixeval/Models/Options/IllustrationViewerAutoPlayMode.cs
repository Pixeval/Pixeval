// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum IllustrationViewerAutoPlayMode
{
    [LocalizedResource(Symbol.ArrowRight, EnumResources.IllustrationViewerAutoPlayMode.Sequential)]
    Sequential,

    [LocalizedResource(Symbol.ArrowRepeatAll, EnumResources.IllustrationViewerAutoPlayMode.Loop)]
    Loop
}
