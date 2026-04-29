// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ApplicationTheme
{
    [LocalizedResource(EnumResources.ApplicationThemeDefault)]
    Default,

    [LocalizedResource(EnumResources.ApplicationThemeLight)]
    Light,

    [LocalizedResource(EnumResources.ApplicationThemeDark)]
    Dark
}
