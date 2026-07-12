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
    private static readonly IReadOnlyList<PixevalHelpTopic> _HelpTopics = Enum.GetValues<PixevalHelpTopic>();

    public PixevalHelpDto Help(PixevalHelpTopic? topic)
    {
        var normalizedTopic = topic ?? PixevalHelpTopic.All;
        return new(normalizedTopic, _HelpTopics, normalizedTopic switch
        {
            PixevalHelpTopic.All =>
            [
                CreateMcpHelp(),
                CreateDownloadMacroHelp(),
                .. CreateWorkFilterHelp(),
                CreateWorkSubscriptionsHelp(),
                CreateExtensionsHelp()
            ],
            PixevalHelpTopic.Mcp => [CreateMcpHelp()],
            PixevalHelpTopic.DownloadMacro => [CreateDownloadMacroHelp()],
            PixevalHelpTopic.WorkFilter => CreateWorkFilterHelp(),
            PixevalHelpTopic.WorkSubscriptions => [CreateWorkSubscriptionsHelp()],
            PixevalHelpTopic.Extensions => [CreateExtensionsHelp()],
            _ => throw new ArgumentOutOfRangeException(nameof(topic))
        });
    }

    private static PixevalHelpDocumentDto CreateMcpHelp() =>
        new(
            PixevalHelpTopic.Mcp,
            I18NManager.GetResource(HelpPageResources.McpHelpExpanderHeader),
            MarkdownResources.McpHelp,
            I18NManager.GetResource(MarkdownResources.McpHelp),
            ["status", "capabilities", "settings_summary", "help"]);

    private static PixevalHelpDocumentDto CreateDownloadMacroHelp() =>
        new(
            PixevalHelpTopic.DownloadMacro,
            I18NManager.GetResource(HelpPageResources.DownloadMacroHelpExpanderHeader),
            MarkdownResources.DownloadMacroHelp,
            I18NManager.GetResource(MarkdownResources.DownloadMacroHelp),
            ["download_macro", "analyze_download_macro", "set_download_macro"]);

    private static IReadOnlyList<PixevalHelpDocumentDto> CreateWorkFilterHelp() =>
    [
        new(
            PixevalHelpTopic.WorkFilter,
            I18NManager.GetResource(HelpPageResources.QueryFilterSimpleHelpHeaderText),
            MarkdownResources.QueryFilterSimpleHelp,
            I18NManager.GetResource(MarkdownResources.QueryFilterSimpleHelp),
            [
                "analyze_work_filter", "search_illustrations", "search_novels", "works", "ranking",
                "bookmarks", "history"
            ]),
        new(
            PixevalHelpTopic.WorkFilter,
            I18NManager.GetResource(HelpPageResources.QueryFilterHelpHeaderText),
            MarkdownResources.QueryFilterHelp,
            I18NManager.GetResource(MarkdownResources.QueryFilterHelp),
            [
                "analyze_work_filter", "search_illustrations", "search_novels", "works", "ranking",
                "bookmarks", "history"
            ])
    ];

    private static PixevalHelpDocumentDto CreateWorkSubscriptionsHelp() =>
        new(
            PixevalHelpTopic.WorkSubscriptions,
            I18NManager.GetResource(HelpPageResources.WorkSubscriptionsHelpExpanderHeader),
            MarkdownResources.WorkSubscriptionsHelp,
            I18NManager.GetResource(MarkdownResources.WorkSubscriptionsHelp),
            ["history", "add_subscription", "remove_subscription", "sync_subscriptions", "download_tasks"]);

    private static PixevalHelpDocumentDto CreateExtensionsHelp() =>
        new(
            PixevalHelpTopic.Extensions,
            I18NManager.GetResource(HelpPageResources.ExtensionsHelpExpanderHeader),
            MarkdownResources.ExtensionsHelp,
            I18NManager.GetResource(MarkdownResources.ExtensionsHelp),
            ["extensions"]);
}
