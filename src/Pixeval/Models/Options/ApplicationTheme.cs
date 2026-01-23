// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Attributes;

namespace Pixeval.Models.Options;

[LocalizationMetadata]
public enum ApplicationTheme
{
    [LocalizedResource(MiscResources.AppThemeSystemDefault)]
    Default,

    [LocalizedResource(MiscResources.AppThemeLight)]
    Light,

    [LocalizedResource(MiscResources.AppThemeDark)]
    Dark
}
