// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Models.Navigation;

public sealed record NavigationParseResult(
    NavigationYamlSettings? Document,
    NavigationConfiguration? Configuration,
    IReadOnlyList<NavigationDiagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(static diagnostic => diagnostic.Severity is NavigationDiagnosticSeverity.Error);

    public bool IsValid => Configuration is not null && !HasErrors;
}
