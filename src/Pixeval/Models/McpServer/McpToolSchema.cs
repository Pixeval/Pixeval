namespace Pixeval.Models.McpServer;

internal sealed record McpToolSchema
{
    public string? Name { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public JsonSchemaObject? InputSchema { get; init; }

    public McpToolItemViewModel? ToViewModel() =>
        string.IsNullOrWhiteSpace(Name)
            ? null
            : new(Name, Title, Description, InputSchema?.ToParameterViewModels() ?? []);
}
