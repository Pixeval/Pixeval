// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Extensions;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Util.IO.Caching;
using Windows.Foundation;

namespace Pixeval.Controls;

public sealed partial class TranslatableTextBlock : Grid
{
    public event TypedEventHandler<TranslatableTextBlock, EventArgs>? IsTranslatedChanged;

    /// <summary>
    /// 原文
    /// </summary>
    [GeneratedDependencyProperty]
    public partial string? Text { get; set; }

    /// <summary>
    /// 已经翻译好的文本，如果不为空则不走翻译流程
    /// </summary>
    [GeneratedDependencyProperty]
    public partial string? PretranslatedText { get; set; }

    /// <summary>
    /// 折叠模式（显示译文时隐藏原文）
    /// </summary>
    [GeneratedDependencyProperty]
    public partial bool IsCompact { get; set; }

    /// <summary>
    /// 是否横向布局
    /// </summary>
    [GeneratedDependencyProperty]
    public partial bool IsHorizontal { get; set; }

    /// <summary>
    /// 当前是否可以翻译（<see cref="Text"/>为空则不行）
    /// </summary>
    [GeneratedDependencyProperty]
    public partial bool CanTranslate { get; set; }

    /// <summary>
    /// 是否正在翻译
    /// </summary>
    [GeneratedDependencyProperty]
    public partial bool IsTranslating { get; private set; }

    /// <summary>
    /// 是否已经显示了翻译内容
    /// </summary>
    [GeneratedDependencyProperty]
    public partial bool IsTranslated { get; private set; }

    /// <summary>
    /// 翻译按钮的水平对齐方式
    /// </summary>
    [GeneratedDependencyProperty(DefaultValue = HorizontalAlignment.Center)]
    public partial HorizontalAlignment HorizontalButtonAlignment { get; set; }

    /// <summary>
    /// 翻译按钮的垂直对齐方式
    /// </summary>
    [GeneratedDependencyProperty(DefaultValue = VerticalAlignment.Center)]
    public partial VerticalAlignment VerticalButtonAlignment { get; set; }

    /// <summary>
    /// 翻译按钮的Margin
    /// </summary>
    [GeneratedDependencyProperty]
    public partial Thickness ButtonMargin { get; set; }

    /// <summary>
    /// 原文普通文本框的样式
    /// </summary>
    public Style? TextBlockStyle { get; set; }

    /// <summary>
    /// 翻译后普通文本框的样式
    /// </summary>
    public Style? TranslatedBlockStyleWhenNotCompact { get; set; }

    /// <summary>
    /// 普通文本框行数
    /// </summary>
    public int MaxLines { get; set; }

    /// <summary>
    /// 文本框翻译的类型
    /// </summary>
    public TextTransformerType TextType { get; set; }

    /// <summary>
    /// 使用Markdown
    /// </summary>
    public bool UseMarkdown { get; set; }

    /// <summary>
    /// 默认的Markdown配置
    /// </summary>
    public static MarkdownConfig StaticMarkdownConfig { get; } = new()
    {
        ImageProvider = new CacheImageProvider()
    };

    /// <summary>
    /// Markdown配置
    /// </summary>
    [field: MaybeNull, AllowNull]
    public MarkdownConfig MarkdownConfig
    {
        get => field ?? StaticMarkdownConfig;
        set;
    }

    public TranslatableTextBlock()
    {
        InitializeComponent();
        MeasureLayout();
    }

    private readonly ExtensionService _extensionService =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    /// <summary>
    /// 在<paramref name="isCompact"/>折叠模式下，若<see cref="TranslationBoxPresenter"/>内容不为空，则隐藏原文
    /// </summary>
    private Visibility CanDisplay(bool isCompact) =>
        isCompact && TranslationBoxPresenter.Content is not null ? Visibility.Collapsed : Visibility.Visible;

    private double GetSpacing(bool canTranslate) => canTranslate ? 5 : 0;

    /// <summary>
    /// 设置<paramref name="maxLines"/>时，将<paramref name="text"/>显示到提示中
    /// </summary>
    private string? NeedToolTip(int maxLines, string? text) => maxLines is 0 ? null : text;

    private object? OriginalTextPresenterContent
    {
        get => OriginalTextPresenter.Content ?? field;
        set
        {
            if (value is null)
            {
                if (OriginalTextPresenter.Content is not null)
                    field = OriginalTextPresenter.Content;
                OriginalTextPresenter.Content = null;
            }
            else
            {
                OriginalTextPresenter.Content = value;
                field = null;
            }
        }
    }

    private object? TranslationBoxPresenterContent
    {
        get => TranslationBoxPresenter.Content ?? field;
        set
        {
            if (value is null)
            {
                if (TranslationBoxPresenter.Content is not null)
                    field = TranslationBoxPresenter.Content;
                TranslationBoxPresenter.Content = null;
            }
            else
            {
                TranslationBoxPresenter.Content = value;
                field = null;
            }

            // 手动触发原始文本框可见性
            OriginalTextPresenter.Visibility = CanDisplay(IsCompact);
            IsTranslated = TranslationBoxPresenter.Content is not null;
        }
    }

    private MarkdownTextBlock GetNewMarkdownTextBlock(MarkdownTextBlock? markdownTextBlock, string text)
    {
        var tb = markdownTextBlock ?? new MarkdownTextBlock();
        tb.Text = text;
        tb.Config = MarkdownConfig;
        return tb;
    }

    private TextBlock GetNewTextBlock(TextBlock? textBlock, string text, Style? textBlockStyle)
    {
        var tb = textBlock ?? new TextBlock();
        tb.Text = text;
        tb.TextWrapping = TextWrapping.Wrap;
        tb.TextTrimming = TextTrimming.CharacterEllipsis;
        tb.IsTextSelectionEnabled = true;
        tb.MaxLines = MaxLines;
        tb.Style = textBlockStyle;
        ToolTipService.SetToolTip(tb, NeedToolTip(MaxLines, text));
        return tb;
    }

    partial void OnTextPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        var canTranslate = !string.IsNullOrWhiteSpace(Text);
        var hasPretranslatedText = !string.IsNullOrWhiteSpace(PretranslatedText);
        OriginalTextPresenterContent = Text is null
            ? null
            : UseMarkdown
                ? GetNewMarkdownTextBlock(OriginalTextPresenterContent as MarkdownTextBlock, Text)
                : GetNewTextBlock(OriginalTextPresenterContent as TextBlock, Text, TextBlockStyle);

        CanTranslate = hasPretranslatedText || canTranslate && _extensionService.ActiveTextTransformerCommands.Any();
        if (!IsTranslated || !hasPretranslatedText)
            TranslationBoxPresenterContent = null;
    }

    private async void GetTranslationClicked(object sender, RoutedEventArgs e)
    {
        if (IsTranslated)
        {
            TranslationBoxPresenterContent = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(PretranslatedText))
        {
            if (_extensionService.ActiveTextTransformerCommands.FirstOrDefault() is not { } translator)
                return;
            IsTranslating = true;
            try
            {
                if (Text is not null)
                {
                    var translatedText = await translator.TransformAsync(Text, TextType);
                    if (translatedText is null)
                        return;
                    TranslationBoxPresenterContent = UseMarkdown
                        ? GetNewMarkdownTextBlock(TranslationBoxPresenterContent as MarkdownTextBlock, translatedText)
                        : GetNewTextBlock(TranslationBoxPresenterContent as TextBlock, translatedText, IsCompact ? TextBlockStyle : TranslatedBlockStyleWhenNotCompact);
                }
            }
            finally
            {
                IsTranslating = false;
            }
        }
        else
        {
            TranslationBoxPresenterContent = UseMarkdown
                ? GetNewMarkdownTextBlock(TranslationBoxPresenterContent as MarkdownTextBlock, PretranslatedText)
                : GetNewTextBlock(TranslationBoxPresenterContent as TextBlock, PretranslatedText, IsCompact ? TextBlockStyle : TranslatedBlockStyleWhenNotCompact);
        }
    }

    partial void OnPretranslatedTextPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        CanTranslate |= !string.IsNullOrWhiteSpace(PretranslatedText);
        TranslationBoxPresenterContent = string.IsNullOrWhiteSpace(PretranslatedText)
            ? null
            : UseMarkdown
                ? GetNewMarkdownTextBlock(TranslationBoxPresenterContent as MarkdownTextBlock, PretranslatedText)
                : GetNewTextBlock(TranslationBoxPresenterContent as TextBlock, PretranslatedText,
                    IsCompact ? TextBlockStyle : TranslatedBlockStyleWhenNotCompact);
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
                SetRow(TranslationBoxPresenter, 0);
            }
            else
            {
                SetRowDefinitionsAuto(2);
                SetRow(TranslationBoxPresenter, 1);
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
                SetRow(TranslationBoxPresenter, 0);
            }
            else
            {
                SetRowDefinitionsAuto(3);
                SetRow(TranslationBoxPresenter, 2);
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

    partial void OnIsTranslatedPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        IsTranslatedChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class CacheImageProvider : IImageProvider
{
    public async Task<Image> GetImage(string url)
    {
        var uri = new UriBuilder(url);
        if (uri.Fragment is ['#', .. var fragment] && int.TryParse(fragment, out var size))
        {
            uri.Fragment = "";
            return new()
            {
                Source = await CacheHelper.GetSourceFromCacheAsync(uri.ToString(), desiredWidth: size),
                Height = size,
                Width = size
            };
        }

        return new() { Source = await CacheHelper.GetSourceFromCacheAsync(url) };
    }

    public bool ShouldUseThisProvider(string url) => true;
}
