// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Models.Navigation;

public sealed record NavigationConfiguration(
    string? NewTabKey,
    NavigationPageDefinition? NewTabPage,
    IReadOnlyList<NavigationMenuItem> HeaderItems,
    IReadOnlyList<NavigationMenuItem> FooterItems)
{
    public NavigationYamlSettings ToYamlSettings() =>
        new()
        {
            NewTab = NewTabKey,
            Header = HeaderItems.Select(child => child.ToYamlItem()).ToArray(),
            Footer = FooterItems.Select(child => child.ToYamlItem()).ToArray()
        };
}
