// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;

namespace Pixeval.Controls;

internal sealed partial class GitHubAlertBlockOverride() : BlockOverride2("BlockquotesEvaluator")
{
    [GeneratedRegex(@"^\s*\[!(?<kind>[A-Za-z]+)\](?<title>[^\n]*)\n?", RegexOptions.Compiled)]
    private static partial Regex AlertHeaderPattern { get; }

    public override IEnumerable<DocumentElement>? Convert2(
        string text,
        Match firstMatch,
        ParseStatus status,
        IMarkdownEngine2 engine,
        out int parseTextBegin,
        out int parseTextEnd)
    {
        parseTextBegin = firstMatch.Index;
        parseTextEnd = firstMatch.Index + firstMatch.Length;

        var content = Unquote(firstMatch.Value);
        var childStatus = new ParseStatus(status.SupportTextAlignment);

        if (TryCreateAlert(content, engine, childStatus, out var alert))
            return [alert];

        var blocks = engine.ParseGamutElement(content + "\n", childStatus);
        return [new BlockquoteElement(blocks)];
    }

    private static string Unquote(string blockquoteText)
    {
        return string.Join(
            "\n",
            blockquoteText
                .Trim('\r', '\n')
                .Split('\n')
                .Select(line =>
                {
                    line = line.TrimEnd('\r');
                    if (line.Length <= 0 || line[0] is not '>')
                        return line;

                    var text = line[1..];
                    return text.StartsWith(' ') ? text[1..] : text;
                }));
    }

    private static bool TryCreateAlert(
        string content,
        IMarkdownEngine2 engine,
        ParseStatus status,
        out GitHubAlertElement alert)
    {
        alert = null!;

        var match = AlertHeaderPattern.Match(content);
        if (!match.Success)
            return false;

        if (!TryNormalizeKind(match.Groups["kind"].Value, out var kind, out var defaultTitle))
            return false;

        var title = match.Groups["title"].Value.Trim();
        if (title.Length is 0)
            title = defaultTitle;

        var body = content[match.Length..].TrimStart('\n');
        var children = body.Length is 0
            ? []
            : engine.ParseGamutElement(body + "\n", status).ToArray();

        alert = new GitHubAlertElement(kind, title, children);
        return true;
    }

    private static bool TryNormalizeKind(string rawKind, out string kind, out string title)
    {
        switch (rawKind.ToUpperInvariant())
        {
            case "INFO":
                kind = "Info";
                title = "Info";
                return true;
            case "NOTE":
                kind = "Note";
                title = "Note";
                return true;
            case "TIP":
                kind = "Tip";
                title = "Tip";
                return true;
            case "IMPORTANT":
                kind = "Important";
                title = "Important";
                return true;
            case "WARNING":
                kind = "Warning";
                title = "Warning";
                return true;
            case "CAUTION":
                kind = "Caution";
                title = "Caution";
                return true;
            default:
                kind = "";
                title = "";
                return false;
        }
    }

    private sealed class GitHubAlertElement : DocumentElement
    {
        private readonly string _kind;
        private readonly CTextBlockElement _title;
        private readonly DocumentElement[] _body;
        private readonly DocumentElement[] _children;
        private readonly Lazy<Border> _control;
        private List<DocumentElement>? _prevSelection;

        public GitHubAlertElement(string kind, string title, DocumentElement[] body)
        {
            _kind = kind;
            _title = new CTextBlockElement([new CRun { Text = title }], "MarkdownAlertTitle");
            _body = body;
            _children = [_title, ..body];
            _control = new Lazy<Border>(Create);
        }

        public override Control Control => _control.Value;

        public override IEnumerable<DocumentElement> Children => _children;

        public override void Select(Point from, Point to)
        {
            var selection = SelectVertical(Control, _children, from, to);

            if (_prevSelection is not null)
            {
                foreach (var previous in _prevSelection)
                {
                    if (!selection.Any(current => ReferenceEquals(current, previous)))
                        previous.UnSelect();
                }
            }

            _prevSelection = selection;
        }

        public override void UnSelect()
        {
            foreach (var child in Children)
                child.UnSelect();

            _prevSelection = null;
        }

        public override void ConstructSelectedText(StringBuilder stringBuilder)
        {
            if (_prevSelection is null)
                return;

            foreach (var child in _prevSelection)
            {
                var previousLength = stringBuilder.Length;
                child.ConstructSelectedText(stringBuilder);

                if (previousLength != stringBuilder.Length && stringBuilder[^1] is not '\n')
                    stringBuilder.Append('\n');
            }
        }

        private static List<DocumentElement> SelectVertical(
            Control anchor,
            IReadOnlyList<DocumentElement> elements,
            Point from,
            Point to)
        {
            if (elements.Count is 0)
                return [];

            var bounds = elements.Select(element => (Element: element, Rect: element.GetRect(anchor))).ToArray();
            var fromIndex = ComputeIndex(bounds, from);
            var toIndex = ComputeIndex(bounds, to);

            return fromIndex <= toIndex
                ? SelectForward(bounds, from, to, fromIndex, toIndex)
                : SelectBackward(bounds, from, to, fromIndex, toIndex);
        }

        private static int ComputeIndex(IReadOnlyList<(DocumentElement Element, Rect Rect)> elements, Point point)
        {
            if (point is { X: <= 0, Y: <= 0 })
                return 0;

            if (double.IsPositiveInfinity(point.X) && double.IsPositiveInfinity(point.Y))
                return elements.Count - 1;

            for (var i = 0; i < elements.Count; i++)
            {
                if (point.Y < elements[i].Rect.Bottom)
                    return i;
            }

            return elements.Count - 1;
        }

        private static List<DocumentElement> SelectForward(
            IReadOnlyList<(DocumentElement Element, Rect Rect)> elements,
            Point from,
            Point to,
            int fromIndex,
            int toIndex)
        {
            var selected = new List<DocumentElement>();
            var localFrom = from;
            var localTo = new Point(double.PositiveInfinity, double.PositiveInfinity);

            for (var i = fromIndex; i <= toIndex; i++)
            {
                if (i == toIndex)
                    localTo = to;

                SelectChild(elements[i], localFrom, localTo);
                selected.Add(elements[i].Element);
                localFrom = default;
            }

            return selected;
        }

        private static List<DocumentElement> SelectBackward(
            IReadOnlyList<(DocumentElement Element, Rect Rect)> elements,
            Point from,
            Point to,
            int fromIndex,
            int toIndex)
        {
            var selected = new List<DocumentElement>();
            var localFrom = from;
            var localTo = default(Point);

            for (var i = fromIndex; i >= toIndex; i--)
            {
                if (i == toIndex)
                    localTo = to;

                SelectChild(elements[i], localFrom, localTo);
                selected.Add(elements[i].Element);
                localFrom = new Point(double.PositiveInfinity, double.PositiveInfinity);
            }

            selected.Reverse();
            return selected;
        }

        private static void SelectChild(
            (DocumentElement Element, Rect Rect) child,
            Point from,
            Point to)
        {
            child.Element.Select(
                new Point(from.X - child.Rect.X, from.Y - child.Rect.Y),
                new Point(to.X - child.Rect.X, to.Y - child.Rect.Y));
        }

        private Border Create()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            panel.Classes.Add("MarkdownAlertContent");
            panel.Classes.Add("MarkdownAlert" + _kind + "Content");

            _title.Control.Classes.Add("MarkdownAlertTitle" + _kind);
            panel.Children.Add(_title.Control);

            foreach (var child in _body)
                panel.Children.Add(child.Control);

            var border = new Border
            {
                Child = panel
            };
            border.Classes.Add("MarkdownAlert");
            border.Classes.Add("MarkdownAlert" + _kind);

            return border;
        }
    }
}
