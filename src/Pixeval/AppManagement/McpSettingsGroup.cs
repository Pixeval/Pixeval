// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;
using FluentIcons.Common;

namespace Pixeval.AppManagement;

public record McpSettingsGroup
{
    public const ushort DefaultPort = 52163;

    // BlobResourceContents is backed by a byte[], so keep the UI limit inside that boundary.
    public const int MaxBinaryResourceMegabytesLimit = 2047;

    [SettingsEntry(Symbol.ServerPlay, AppSettingsResources.EnableMcpServerEntry.Header, AppSettingsResources.EnableMcpServerEntry.Description)]
    public bool EnableServer { get; set; }

    [SettingsEntry(Symbol.SerialPort, AppSettingsResources.McpPortEntry.Header, AppSettingsResources.McpPortEntry.Description)]
    public ushort Port { get; set; } = DefaultPort;

    [SettingsEntry(Symbol.CalligraphyPenCheckmark, AppSettingsResources.EnableMcpWriteToolsEntry.Header, AppSettingsResources.EnableMcpWriteToolsEntry.Description)]
    public bool EnableWriteTools { get; set; }

    [SettingsEntry(Symbol.DatabaseCheckmark, AppSettingsResources.McpMaxBinaryResourceMegabytesEntry.Header,
        AppSettingsResources.McpMaxBinaryResourceMegabytesEntry.Description)]
    public int MaxBinaryResourceMegabytes { get; set; } = 50;
}
