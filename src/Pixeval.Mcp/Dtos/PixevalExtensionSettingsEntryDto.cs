// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalExtensionSettingsEntryDto(
    string Token,
    string Header,
    string Description,
    string ValueKind,
    string? Placeholder,
    string? DescriptionUri,
    double? Min,
    double? Max,
    double? Step,
    IReadOnlyList<PixevalExtensionEnumItemDto> EnumItems);