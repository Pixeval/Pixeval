// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;
using FluentIcons.Common;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record McpSettingsGroup
{
    public const ushort DefaultPort = 52163;

    // BlobResourceContents is backed by a byte[], so keep the UI limit inside that boundary.
    public const int MaxBinaryResourceMegabytesLimit = 2047;

    [SettingsEntry(Symbol.ServerPlay, EnableMcpServerEntryHeader, EnableMcpServerEntryDescription)]
    public bool EnableServer { get; set; }

    [SettingsEntry(Symbol.SerialPort, McpPortEntryHeader, McpPortEntryDescription)]
    public ushort Port { get; set; } = DefaultPort;

    [SettingsEntry(Symbol.CalligraphyPenCheckmark, EnableMcpWriteToolsEntryHeader, EnableMcpWriteToolsEntryDescription)]
    public bool EnableWriteTools { get; set; }

    [SettingsEntry(Symbol.DatabaseCheckmark, McpMaxBinaryResourceMegabytesEntryHeader,
        McpMaxBinaryResourceMegabytesEntryDescription)]
    public int MaxBinaryResourceMegabytes { get; set; } = 50;
}
