// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;

namespace Pixeval.Models.Navigation;

public sealed record NavigationParseResult(
    NavigationYamlSettings? Document,
    NavigationConfiguration? Configuration,
    IReadOnlyList<NavigationDiagnostic> Diagnostics)
{
    public bool IsValid => Configuration is not null && Diagnostics.Count is 0;
}
