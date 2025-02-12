// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions;
using Pixeval.Extensions.Common.Commands.Transformers;
using Microsoft.UI.Xaml.Documents;
using Pixeval.Util.IO.Caching;

namespace Pixeval.Controls;

public sealed partial class TranslatableTextBlock : Grid
{
    [GeneratedDependencyProperty]
    public partial object? Text { get; set; }

    [GeneratedDependencyProperty]
    public partial bool IsCompact { get; set; }

    [GeneratedDependencyProperty]
    public partial bool IsHorizontal { get; set; }

    [GeneratedDependencyProperty]
    public partial bool CanTranslate { get; private set; }

    [GeneratedDependencyProperty]
    public partial bool IsTranslating { get; private set; }

    [GeneratedDependencyProperty(DefaultValue = "")]
    public partial string? TranslatedText { get; private set; }

    [GeneratedDependencyProperty]
    public partial Style? TextBlockStyle { get; set; }

    [GeneratedDependencyProperty]
    public partial int MaxLines { get; set; }

    [GeneratedDependencyProperty(DefaultValue = HorizontalAlignment.Center)]
    public partial HorizontalAlignment HorizontalButtonAlignment { get; set; }

    [GeneratedDependencyProperty(DefaultValue = VerticalAlignment.Center)]
    public partial VerticalAlignment VerticalButtonAlignment { get; set; }

    public TextTransformerType TextType { get; set; }

    public bool TranslateRespectively { get; set; } = true;

    public TranslatableTextBlock()
    {
        InitializeComponent();
        MeasureLayout();
    }

    private readonly ExtensionService _extensionService =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    /// <summary>
    /// 在<paramref name="isCompact"/>折叠模式下，若<paramref name="translatedText"/>翻译文本不为空，则隐藏原文
    /// </summary>
    private Visibility CanDisplay(string? translatedText, bool isCompact) =>
        isCompact && !string.IsNullOrWhiteSpace(translatedText) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// 设置<paramref name="maxLines"/>时，将<paramref name="text"/>显示到提示中
    /// </summary>
    private string? NeedToolTip(int maxLines, string? text) => maxLines is 0 ? null : text;

    async partial void OnTextPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        var canTranslate = false;
        switch (Text)
        {
            case string text:
                canTranslate = !string.IsNullOrWhiteSpace(text);
                var tb = new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    IsTextSelectionEnabled = true,
                    MaxLines = MaxLines,
                    Style = TextBlockStyle
                };
                ToolTipService.SetToolTip(tb, NeedToolTip(MaxLines, text));
                OriginalTextPresenter.Content = tb;
                break;
            case IEnumerable<InlineContent> contents:
                var arr = contents.ToArray();
                if (arr.Length is 0)
                {
                    canTranslate = false;
                    break;
                }

                if (arr.All(t => t is RunContent))
                {
                    canTranslate = true;
                    var text = arr.Cast<RunContent>().Aggregate("", (current, content) => current + content.Text);
                    var textBlock = new TextBlock
                    {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        IsTextSelectionEnabled = true,
                        MaxLines = MaxLines,
                        Style = TextBlockStyle
                    };
                    ToolTipService.SetToolTip(textBlock, NeedToolTip(MaxLines, text));
                    OriginalTextPresenter.Content = textBlock;
                }
                else
                {
                    canTranslate = !arr.All(t => t is ImageContent);

                    foreach (var content in arr)
                    {
                        var paragraph = new Paragraph();
                        switch (content)
                        {
                            case RunContent runContent:
                                paragraph.Inlines.Add(new Run { Text = runContent.Text });
                                break;
                            case ImageContent imageContent:
                                paragraph.Inlines.Add(new InlineUIContainer
                                {
                                    Child = new Image
                                    {
                                        VerticalAlignment = VerticalAlignment.Bottom,
                                        Source = await CacheHelper.GetSourceFromCacheAsync(imageContent.Content,
                                                desiredWidth: 24),
                                        Width = 24,
                                        Height = 24
                                    }
                                }
                                );
                                break;
                        }

                        OriginalTextPresenter.Content = new RichTextBlock
                        {
                            Blocks = { paragraph },
                            TextWrapping = TextWrapping.Wrap,
                            TextTrimming = TextTrimming.CharacterEllipsis,
                            IsTextSelectionEnabled = true,
                            MaxLines = MaxLines
                        };
                    }
                }

                break;
            default:
                canTranslate = false;
                OriginalTextPresenter.Content = null;
                break;
        }

        TranslatedText = "";
        CanTranslate = canTranslate && _extensionService.ActiveTextTransformerCommands.Any();
    }

    private async void GetTranslationClicked(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TranslatedText))
        {
            // 赋值null会导致不触发绑定变化真离谱了
            TranslatedText = "";
            return;
        }

        if (_extensionService.ActiveTextTransformerCommands.FirstOrDefault() is not { } translator)
            return;
        IsTranslating = true;
        try
        {
            switch (Text)
            {
                case string text:
                    TranslatedText = await translator.TransformAsync(text, TextType);
                    break;
                case IEnumerable<InlineContent> contents:
                    var arr = contents.OfType<RunContent>().ToArray();
                    if (TranslateRespectively)
                    {
                        var result = "";
                        foreach (var content in arr)
                            result += await translator.TransformAsync(content.Text, TextType);
                        TranslatedText = result;
                    }
                    else
                    {
                        var text = arr.Aggregate("", (current, content) => current + content.Text);
                        TranslatedText = await translator.TransformAsync(text, TextType);
                    }

                    break;
            }
        }
        finally
        {
            IsTranslating = false;
        }
    }

    partial void OnIsCompactPropertyChanged(DependencyPropertyChangedEventArgs e) => MeasureLayout();

    partial void OnIsHorizontalPropertyChanged(DependencyPropertyChangedEventArgs e) => MeasureLayout();

    /// <summary>
    /// <code>
    /// ┌--------------------------┬------------------┐
    /// |           1 *            |       Auto       |
    /// └--------------------------┴------------------┘
    /// Vertical Normal
    /// ┌--------------------------┐
    /// | Original Text            |
    /// ├--------------------------┤
    /// | Translate Button         |
    /// ├--------------------------┤
    /// | Translated Text          |
    /// └--------------------------┘
    /// Vertical Compact
    /// ┌--------------------------┐
    /// | Original/Translated Text |
    /// ├--------------------------┤
    /// | Translate Button         |
    /// └--------------------------┘
    /// Horizontal Normal
    /// ┌--------------------------┬------------------┐
    /// | Original Text            | Translate Button |
    /// ├--------------------------┼------------------┤
    /// | Translated Text          |                  |
    /// └--------------------------┴------------------┘
    /// Horizontal Compact
    /// ┌--------------------------┬------------------┐
    /// | Original/Translated Text | Translate Button |
    /// └--------------------------┴------------------┘
    /// </code>
    /// </summary>
    private void MeasureLayout()
    {
        if (IsHorizontal)
        {
            if (ColumnDefinitions.Count is not 2)
            {
                ColumnDefinitions.Clear();
                ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            }
            SetRow(TranslateButton, 0);
            SetColumn(TranslateButton, 1);

            if (IsCompact)
            {
                SetRowDefinitionsAuto(1);
                SetRow(TranslationBox, 0);
            }
            else
            {
                SetRowDefinitionsAuto(2);
                SetRow(TranslationBox, 1);
            }
        }
        else
        {
            if (ColumnDefinitions.Count is not 1)
            {
                ColumnDefinitions.Clear();
                ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
            }
            SetRow(TranslateButton, 1);
            SetColumn(TranslateButton, 0);

            if (IsCompact)
            {
                SetRowDefinitionsAuto(2);
                SetRow(TranslationBox, 0);
            }
            else
            {
                SetRowDefinitionsAuto(3);
                SetRow(TranslationBox, 2);
            }
        }
    }

    private void SetRowDefinitionsAuto(int row)
    {
        if (RowDefinitions.Count != row)
        {
            RowDefinitions.Clear();
            for (var i = 0; i < row; ++i)
                RowDefinitions.Add(new() { Height = GridLength.Auto });
        }
    }
}

public abstract record InlineContent(string Content);

public record RunContent(string Text) : InlineContent(Text);

public record ImageContent(string Url) : InlineContent(Url);
