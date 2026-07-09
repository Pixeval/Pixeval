// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Pixeval.Models.Navigation;

namespace Pixeval.Views.Settings;

public sealed class NavigationYamlEditorColorizer : DocumentColorizingTransformer
{
    private static readonly SolidColorBrush _KeyBrush = new(Color.Parse("#569CD6"));
    private static readonly SolidColorBrush _StringBrush = new(Color.Parse("#CE9178"));
    private static readonly SolidColorBrush _LiteralBrush = new(Color.Parse("#B5CEA8"));
    private static readonly SolidColorBrush _CommentBrush = new(Color.Parse("#6A9955"));
    private static readonly SolidColorBrush _ErrorBackgroundBrush = new(Color.FromArgb(72, 224, 82, 82));
    private static readonly SolidColorBrush _WarningBackgroundBrush = new(Color.FromArgb(64, 255, 185, 0));

    private IReadOnlyList<NavigationDiagnostic> _diagnostics = [];
    private int _documentLength;

    public void Update(IReadOnlyList<NavigationDiagnostic> diagnostics, int documentLength)
    {
        _diagnostics = diagnostics;
        _documentLength = documentLength;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        ColorizeSyntax(line);
        ColorizeDiagnostics(line);
    }

    private void ColorizeSyntax(DocumentLine line)
    {
        var text = CurrentContext.Document.GetText(line);
        var keyStart = 0;
        while (keyStart < text.Length && char.IsWhiteSpace(text[keyStart]))
            keyStart++;

        if (keyStart < text.Length && text[keyStart] is '-')
        {
            keyStart++;
            while (keyStart < text.Length && char.IsWhiteSpace(text[keyStart]))
                keyStart++;
        }

        var keyEnd = keyStart;
        while (keyEnd < text.Length && (char.IsLetterOrDigit(text[keyEnd]) || text[keyEnd] is '_' or '-'))
            keyEnd++;

        var colon = keyEnd < text.Length ? text.IndexOf(':', keyEnd) : -1;
        if (keyEnd > keyStart && colon >= 0)
        {
            ChangeLinePart(
                line.Offset + keyStart,
                line.Offset + keyEnd,
                element => element.TextRunProperties.SetForegroundBrush(_KeyBrush));

            var valueStart = colon + 1;
            while (valueStart < text.Length && char.IsWhiteSpace(text[valueStart]))
                valueStart++;

            var commentStart = FindCommentStart(text, valueStart);
            var valueEnd = commentStart >= 0 ? commentStart : text.Length;
            if (valueStart < valueEnd)
            {
                var brush = text[valueStart] is '"' or '\''
                    ? _StringBrush
                    : _LiteralBrush;
                ChangeLinePart(
                    line.Offset + valueStart,
                    line.Offset + valueEnd,
                    element => element.TextRunProperties.SetForegroundBrush(brush));
            }
        }

        var comment = FindCommentStart(text, 0);
        if (comment >= 0)
        {
            ChangeLinePart(
                line.Offset + comment,
                line.EndOffset,
                element => element.TextRunProperties.SetForegroundBrush(_CommentBrush));
        }
    }

    private void ColorizeDiagnostics(DocumentLine line)
    {
        foreach (var diagnostic in _diagnostics)
        {
            var (start, end) = Clamp(diagnostic.Start, diagnostic.Length, _documentLength);
            var intersectStart = int.Max(start, line.Offset);
            var intersectEnd = int.Min(end, line.EndOffset);
            if (intersectStart >= intersectEnd)
                continue;

            var brush = diagnostic.Severity is NavigationDiagnosticSeverity.Warning
                ? _WarningBackgroundBrush
                : _ErrorBackgroundBrush;
            ChangeLinePart(intersectStart, intersectEnd, element => element.TextRunProperties.SetBackgroundBrush(brush));
        }
    }

    private static int FindCommentStart(string text, int start)
    {
        var inSingleQuote = false;
        var inDoubleQuote = false;
        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];
            if (c is '\'' && !inDoubleQuote)
                inSingleQuote = !inSingleQuote;
            else if (c is '"' && !inSingleQuote)
                inDoubleQuote = !inDoubleQuote;
            else if (c is '#' && !inSingleQuote && !inDoubleQuote)
                return i;
        }

        return -1;
    }

    private static (int Start, int End) Clamp(int start, int length, int documentLength)
    {
        if (documentLength <= 0)
            return (0, 0);

        var safeStart = int.Clamp(start, 0, documentLength - 1);
        var safeEnd = int.Clamp(start + int.Max(length, 1), safeStart + 1, documentLength);
        return (safeStart, safeEnd);
    }
}
