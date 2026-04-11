// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Markdown.Avalonia;
using Markdown.Avalonia.Html;
using Markdown.Avalonia.StyleCollections;

namespace Pixeval.Controls;

public class MarkdownBox : MarkdownScrollViewer
{
    public MarkdownBox()
    {
        // 或MarkdownStyleFluentAvalonia
        MarkdownStyle = new MarkdownStyleFluentTheme();
        Plugins.Plugins.Add(new HtmlPlugin());
    }
}
