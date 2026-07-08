// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Download.MacroParser;
using Pixeval.I18N;
using Pixeval.Mcp.Dtos;
using Pixeval.Models.Download;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.McpServer;

public sealed partial class PixevalMcpService
{
    public PixevalDownloadMacroSettingsDto DownloadMacro()
    {
        var current = ViewModel.AppSettings.DownloadSettings.DownloadPathMacro;
        return new(
            current,
            AnalyzeDownloadMacro(current),
            [.. DownloadPathMacroParser.MacroProvider.Select(ToDownloadMacroDefinitionDto)]);
    }

    public PixevalDownloadMacroAnalysisDto AnalyzeDownloadMacro(string text)
    {
        var normalized = text.ReplaceLineEndings("").Trim();
        var analysis = DownloadPathMacroParser.Analyze(normalized);
        return new(
            normalized,
            analysis.IsSuccess,
            [.. analysis.Diagnostics.Select(ToDownloadMacroDiagnosticDto)],
            [.. analysis.Highlights.Select(ToDownloadMacroHighlightDto)]);
    }

    public PixevalSetDownloadMacroResultDto SetDownloadMacro(string text)
    {
        var previous = ViewModel.AppSettings.DownloadSettings.DownloadPathMacro;
        var normalized = text.ReplaceLineEndings("").Trim();
        var analysis = AnalyzeDownloadMacro(normalized);

        if (string.IsNullOrWhiteSpace(normalized))
            return new(false, "Download macro cannot be empty.", previous, previous, analysis);

        if (!analysis.IsSuccess)
            return new(false, analysis.Diagnostics is [{ Message: { } message }, ..] ? message : "Download macro is invalid.", previous,
                previous, analysis);

        try
        {
            _ = ReduceDownloadMacroPath(normalized, DesignHelper.DownloadParserSampleWork(ImageType.SingleImage), true);
            _ = ReduceDownloadMacroPath(normalized, DesignHelper.DownloadParserSampleWork(ImageType.ImageSet), true);
            _ = ReduceDownloadMacroPath(normalized, DesignHelper.DownloadParserSampleWork(ImageType.Other), true);
        }
        catch (Exception e)
        {
            return new(false, $"Download macro cannot be reduced: {e.Message}", previous, previous, analysis);
        }

        ViewModel.AppSettings.DownloadSettings.DownloadPathMacro = normalized;
        AppInfo.SaveSettings(ViewModel.AppSettings);
        return new(true, "Download macro updated.", previous, normalized, analysis);
    }

    private static string ReduceDownloadMacroPath(string text, IArtworkInfo artwork, bool forcePngExtension)
    {
        var path = IoHelper.NormalizePath(DownloadPathMacroParser.Reduce(text, new ParserContext(artwork)));
        var setIndex = artwork.TryGetSetIndex();
        if (setIndex >= 0)
            path = IoHelper.ReplaceTokenSetIndex(path, setIndex);
        return forcePngExtension ? IoHelper.ChangeExtension(path, ".png") : path;
    }

    private static PixevalDownloadMacroDefinitionDto ToDownloadMacroDefinitionDto(IMacro macro)
    {
        var (kind, contextType) = macro switch
        {
            ITransducer transducer => ("transducer", transducer.ContextType.Name),
            IPredicate predicate => ("predicate", predicate.ContextType.Name),
            _ => ("macro", "")
        };
        return new(
            macro.Name,
            macro.Description,
            kind,
            contextType,
            macro is IContextRestrictedMacro restricted ? restricted.RequiredPredicateName : null);
    }

    private static PixevalDownloadMacroDiagnosticDto ToDownloadMacroDiagnosticDto(MacroDiagnostic diagnostic) =>
        new(
            diagnostic.Kind.ToString(),
            diagnostic.Span.Start,
            diagnostic.Span.Length,
            FormatDiagnostic(diagnostic),
            [.. diagnostic.Arguments.Select(argument => argument?.ToString() ?? "")]);

    private static PixevalDownloadMacroHighlightDto ToDownloadMacroHighlightDto(MacroHighlightSpan highlight) =>
        new(
            highlight.Kind.ToString(),
            highlight.Span.Start,
            highlight.Span.Length,
            highlight.NestingDepth);

    private static string FormatDiagnostic(MacroDiagnostic diagnostic)
    {
        var arguments = diagnostic.Arguments;
        var formattedDiagnostic = arguments.Count > 0
            ? I18NManager.GetResource(GetMacroDiagnosticResourceKey(diagnostic.Kind), [.. arguments])
            : I18NManager.GetResource(GetMacroDiagnosticResourceKey(diagnostic.Kind));
        return I18NManager.GetResource(
            MacroParserResources.MessageWithPositionFormatted,
            formattedDiagnostic,
            diagnostic.Span.Start + 1);
    }

    private static string GetMacroDiagnosticResourceKey(MacroDiagnosticKind kind) => "MacroParser." + kind;
}
