// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalHelpDto(
    PixevalHelpTopic Topic,
    IReadOnlyList<PixevalHelpTopic> AvailableTopics,
    IReadOnlyList<PixevalHelpDocumentDto> Documents);
