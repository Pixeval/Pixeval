// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Models.Navigation;

public enum NavigationDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record NavigationDiagnostic(
    NavigationDiagnosticSeverity Severity,
    string Message,
    int Start,
    int Length,
    int Line,
    int Column)
{
    public string PositionText => Line > 0 && Column > 0
        ? $"{Line}:{Column}"
        : "";
}
