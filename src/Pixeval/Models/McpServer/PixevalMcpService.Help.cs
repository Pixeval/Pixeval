// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Pixeval.I18N;
using Pixeval.Mcp;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Models.McpServer;

public sealed partial class PixevalMcpService
{
    private static readonly IReadOnlyList<string> _HelpTopics =
    [
        "all",
        "mcp",
        "download_macro",
        "work_filter",
        "extensions"
    ];

    public PixevalHelpDto Help(PixevalHelpTopic? topic)
    {
        topic ??= PixevalHelpTopic.All;
        var normalized = ToHelpTopicName(topic.Value);
        return new(normalized, _HelpTopics, normalized switch
        {
            "all" =>
            [
                CreateMcpHelp(),
                CreateDownloadMacroHelp(),
                .. CreateWorkFilterHelp(),
                CreateExtensionsHelp()
            ],
            "mcp" => [CreateMcpHelp()],
            "download_macro" => [CreateDownloadMacroHelp()],
            "work_filter" => CreateWorkFilterHelp(),
            "extensions" => [CreateExtensionsHelp()],
            _ => throw new ArgumentOutOfRangeException(nameof(topic))
        });
    }

    private static PixevalHelpDocumentDto CreateMcpHelp() =>
        new(
            "mcp",
            I18NManager.GetResource(HelpPageResources.McpHelpExpanderHeader),
            MarkdownResources.McpHelp,
            I18NManager.GetResource(MarkdownResources.McpHelp),
            ["status", "capabilities", "settings_summary", "help"]);

    private static PixevalHelpDocumentDto CreateDownloadMacroHelp() =>
        new(
            "download_macro",
            I18NManager.GetResource(HelpPageResources.DownloadMacroHelpExpanderHeader),
            MarkdownResources.DownloadMacroHelp,
            I18NManager.GetResource(MarkdownResources.DownloadMacroHelp),
            ["download_macro", "analyze_download_macro", "set_download_macro"]);

    private static IReadOnlyList<PixevalHelpDocumentDto> CreateWorkFilterHelp() =>
    [
        new(
            "work_filter",
            I18NManager.GetResource(HelpPageResources.QueryFilterSimpleHelpHeaderText),
            MarkdownResources.QueryFilterSimpleHelp,
            I18NManager.GetResource(MarkdownResources.QueryFilterSimpleHelp),
            [
                "analyze_work_filter", "search_illustrations", "search_novels", "works", "ranking",
                "bookmarks", "history"
            ]),
        new(
            "work_filter",
            I18NManager.GetResource(HelpPageResources.QueryFilterHelpHeaderText),
            MarkdownResources.QueryFilterHelp,
            I18NManager.GetResource(MarkdownResources.QueryFilterHelp),
            [
                "analyze_work_filter", "search_illustrations", "search_novels", "works", "ranking",
                "bookmarks", "history"
            ])
    ];

    private static PixevalHelpDocumentDto CreateExtensionsHelp() =>
        new(
            "extensions",
            I18NManager.GetResource(HelpPageResources.ExtensionsHelpExpanderHeader),
            MarkdownResources.ExtensionsHelp,
            I18NManager.GetResource(MarkdownResources.ExtensionsHelp),
            ["extensions"]);

    private static string ToHelpTopicName(PixevalHelpTopic topic) =>
        topic switch
        {
            PixevalHelpTopic.All => "all",
            PixevalHelpTopic.Mcp => "mcp",
            PixevalHelpTopic.DownloadMacro => "download_macro",
            PixevalHelpTopic.WorkFilter => "work_filter",
            PixevalHelpTopic.Extensions => "extensions",
            _ => throw new ArgumentOutOfRangeException(nameof(topic))
        };
}
