// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public sealed partial class DocumentViewer
{
    /// <summary>
    /// 页宽
    /// </summary>
    [GeneratedDependencyProperty(DefaultValue = 1000d)]
    public partial double NovelMaxWidth { get; set; }

    /// <summary>
    /// 行高
    /// </summary>
    [GeneratedDependencyProperty(DefaultValue = 28d)]
    public partial double LineHeight { get; set; }

    /// <summary>
    /// ViewModel
    /// </summary>
    [GeneratedDependencyProperty]
    public partial List<Paragraph>? Paragraphs { get; set; }

    public DocumentViewer() => InitializeComponent();

    partial void OnParagraphsPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        NovelRichTextBlock.Blocks.Clear();
        if (Paragraphs is not null)
            NovelRichTextBlock.Blocks.AddRange(Paragraphs);
    }
}
