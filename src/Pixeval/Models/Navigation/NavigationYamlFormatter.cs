// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using SharpYaml;

namespace Pixeval.Models.Navigation;

public static class NavigationYamlFormatter
{
    public static string Format(NavigationConfiguration configuration) =>
        YamlSerializer
            .Serialize(configuration.ToYamlSettings(), NavigationYamlSerializerContext.Default.NavigationYamlSettings)
            .TrimEnd() + Environment.NewLine;
}
