// Copyright (c) Pixeval.Mcp.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Mcp.Dtos;

public sealed record PixevalExtensionHostDto(
    string Name,
    string Description,
    string Author,
    string Version,
    bool IsActive,
    bool IsPendingUninstall,
    int Priority,
    string? Link,
    string? HelpLink,
    string HostLibraryName,
    string UninstallTarget,
    IReadOnlyList<string> ExtensionTypes,
    IReadOnlyList<PixevalExtensionSettingsEntryDto> Settings);