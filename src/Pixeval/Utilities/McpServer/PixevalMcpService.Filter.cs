// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;
using Mako.Model;
using Pixeval.Filters.Analysis;
using Pixeval.I18N;
using Pixeval.Mcp;
using Pixeval.Mcp.Dtos;
using Pixeval.Models.Filters;

namespace Pixeval.Utilities.McpServer;

public sealed partial class PixevalMcpService
{
    private const string FilterDiagnosticResourcePrefix = "Filter.Diagnostics.";

    public PixevalWorkFilterAnalysisDto AnalyzeWorkFilter(string? text, int caretPosition)
    {
        var normalized = text ?? "";
        var caret = caretPosition < 0 ? normalized.Length : int.Clamp(caretPosition, 0, normalized.Length);
        var analysis = WorkFilterLanguage.Instance.Analyze(normalized, caret);
        return ToFilterAnalysisDto(normalized, caret, analysis);
    }

    public IReadOnlyList<WorkBase> FilterWorks(IReadOnlyList<WorkBase> works, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return works;

        var analysis = WorkFilterLanguage.Instance.Analyze(text);
        if (!analysis.IsSuccess || analysis.Query is not { } query)
            throw new PixevalMcpException(analysis.Diagnostics.Count is 0
                ? "Pixeval work filter is invalid."
                : FormatDiagnostic(analysis.Diagnostics[0]));

        return
        [
            .. works.Where(work => work is Misaki.IArtworkInfo artwork
                                   && WorkFilterEvaluator.Filter(artwork, query.Root))
        ];
    }

    private static PixevalWorkFilterAnalysisDto ToFilterAnalysisDto(
        string text,
        int caretPosition,
        FilterAnalysisResult analysis) =>
        new(
            text,
            caretPosition,
            analysis.IsSuccess,
            analysis.Query is { } query ? new PixevalWorkFilterQueryDto(query.HasPredicates) : null,
            [.. analysis.Diagnostics.Select(ToFilterDiagnosticDto)],
            [.. analysis.Completions.Select(ToFilterCompletionDto)]);

    private static PixevalWorkFilterDiagnosticDto ToFilterDiagnosticDto(FilterDiagnostic diagnostic) =>
        new(
            diagnostic.Kind.ToString(),
            diagnostic.Span.Start,
            diagnostic.Span.Length,
            FormatDiagnostic(diagnostic),
            [.. diagnostic.Arguments.Select(argument => argument?.ToString() ?? "")]);

    private static PixevalWorkFilterCompletionDto ToFilterCompletionDto(FilterCompletionItem completion) =>
        new(
            completion.DisplayText,
            completion.InsertText,
            completion.ReplacementSpan.Start,
            completion.ReplacementSpan.Length,
            completion.Description,
            completion.IsHintOnly);

    private static string FormatDiagnostic(FilterDiagnostic diagnostic)
    {
        var arguments = diagnostic.Arguments;
        return arguments.Count > 0
            ? I18NManager.GetResource(GetFilterDiagnosticResourceKey(diagnostic.Kind), [.. arguments])
            : I18NManager.GetResource(GetFilterDiagnosticResourceKey(diagnostic.Kind));
    }

    private static string GetFilterDiagnosticResourceKey(FilterDiagnosticKind kind) =>
        FilterDiagnosticResourcePrefix + kind;
}
