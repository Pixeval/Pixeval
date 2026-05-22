using System;
using System.Collections.Generic;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Pixeval.Download.MacroParser;

namespace Pixeval.Controls.Settings;

public sealed class MacroEditorColorizer : DocumentColorizingTransformer
{
    private static readonly SolidColorBrush[] _HighlightBrushes =
    [
        new(Color.Parse("#C086C0")),
        new(Color.Parse("#9AC6CE")),
        new(Color.Parse("#DCDCA3")),
        new(Color.Parse("#45A15E"))
    ];

    private static readonly SolidColorBrush _ErrorBackgroundBrush = new(Color.FromArgb(72, 224, 82, 82));

    private IReadOnlyList<MacroHighlightSpan> _highlights = [];
    private IReadOnlyList<MacroDiagnostic> _diagnostics = [];
    private int _documentLength;

    public void Update(MacroAnalysisResult analysis, int documentLength)
    {
        _highlights = analysis.Highlights;
        _diagnostics = analysis.Diagnostics;
        _documentLength = documentLength;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        foreach (var highlight in _highlights)
        {
            if (!TryIntersect(highlight.Span, line, out var start, out var end))
                continue;

            var brush = _HighlightBrushes[highlight.NestingDepth % _HighlightBrushes.Length];
            ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(brush));
        }

        foreach (var diagnostic in _diagnostics)
        {
            var span = Clamp(diagnostic.Span, _documentLength);
            if (!TryIntersect(span, line, out var start, out var end))
                continue;

            ChangeLinePart(start, end, element => element.TextRunProperties.SetBackgroundBrush(_ErrorBackgroundBrush));
        }
    }

    private static MacroTextSpan Clamp(MacroTextSpan span, int documentLength)
    {
        if (documentLength <= 0)
            return new MacroTextSpan(0, 0);

        var safeStart = Math.Clamp(span.Start, 0, documentLength - 1);
        var safeEnd = Math.Clamp(Math.Max(span.End, safeStart + 1), safeStart + 1, documentLength);
        return new MacroTextSpan(safeStart, safeEnd - safeStart);
    }

    private static bool TryIntersect(MacroTextSpan span, DocumentLine line, out int start, out int end)
    {
        start = Math.Max(span.Start, line.Offset);
        end = Math.Min(span.End, line.EndOffset);
        return start < end;
    }
}
