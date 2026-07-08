using System;
using System.Collections.Generic;

namespace Pixeval.Models.McpServer;

public sealed record McpToolItemViewModel(
    string Name,
    string? Title,
    string? Description,
    IReadOnlyList<McpToolParameterItemViewModel> Parameters)
{
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title) && !string.Equals(Title, Name, StringComparison.Ordinal);

    public string TitleText => Title ?? "";

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public string DescriptionText => Description ?? "";

    public bool HasParameters => Parameters.Count is not 0;

    public bool HasNoParameters => Parameters.Count is 0;
}
