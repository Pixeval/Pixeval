namespace Pixeval.Models.McpServer;

public sealed record McpToolParameterItemViewModel(string Signature, string? Description)
{
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public string DescriptionText => Description ?? "";
}
