// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Mcp.Dtos;

namespace Pixeval.Models.McpServer;

public sealed partial class PixevalMcpService
{
    public PixevalSettingsSummaryDto SettingsSummary() =>
        ViewModel.AppSettings.ToMcpDto();
}
