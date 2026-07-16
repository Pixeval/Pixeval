// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using ColorTextBlock.Avalonia;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Markdown.Avalonia;
using Markdown.Avalonia.StyleCollections;
using Markdown.Avalonia.Svg;
using Markdown.Avalonia.SyntaxHigh;
using Markdown.Avalonia.Utils;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Pixeval.Utilities.GitHub;
using Pixeval.Utilities.IO.Caching;
using Pixeval.Views.Viewers;

namespace Pixeval.Controls;

public sealed class MarkdownHyperlinkClickedEventArgs(string url) : EventArgs
{
    public string Url { get; } = url;

    public bool Handled { get; set; }
}

public class MarkdownBox : MarkdownScrollViewer
{
    private const double AnchorScrollOffset = 12;
    private const string CodePadTypeName = "Markdown.Avalonia.SyntaxHigh.CodePad";
    private const string CodeBlockClass = "CodeBlock";
    private const string CodeBlockRootClass = "PixevalCodeBlockRoot";
    private const string CodeBlockToolbarClass = "PixevalCodeBlockToolbar";
    private const string InlineCodeNormalizedClass = "PixevalInlineCodeNormalized";
    private const string InlineCodeSentinelClass = "PixevalInlineCodeSentinel";
    private static readonly Cursor TextCursor = new(StandardCursorType.Ibeam);

    public static readonly StyledProperty<IBrush?> MarkdownForegroundProperty =
        AvaloniaProperty.Register<MarkdownBox, IBrush?>(nameof(MarkdownForeground));

    public static readonly StyledProperty<FontFamily?> MarkdownFontFamilyProperty =
        AvaloniaProperty.Register<MarkdownBox, FontFamily?>(nameof(MarkdownFontFamily));

    public static readonly StyledProperty<FontWeight?> MarkdownFontWeightProperty =
        AvaloniaProperty.Register<MarkdownBox, FontWeight?>(nameof(MarkdownFontWeight));

    public static readonly StyledProperty<double> MarkdownFontSizeProperty =
        AvaloniaProperty.Register<MarkdownBox, double>(nameof(MarkdownFontSize), double.NaN);

    public static readonly StyledProperty<double> MarkdownLineHeightProperty =
        AvaloniaProperty.Register<MarkdownBox, double>(nameof(MarkdownLineHeight), double.NaN);

    public static readonly StyledProperty<double> MarkdownLineSpacingProperty =
        AvaloniaProperty.Register<MarkdownBox, double>(nameof(MarkdownLineSpacing), double.NaN);

    private bool _presentationApplyQueued;

    public event EventHandler<MarkdownHyperlinkClickedEventArgs>? HyperlinkClicked;

    public IBrush? MarkdownForeground
    {
        get => GetValue(MarkdownForegroundProperty);
        set => SetValue(MarkdownForegroundProperty, value);
    }

    public FontFamily? MarkdownFontFamily
    {
        get => GetValue(MarkdownFontFamilyProperty);
        set => SetValue(MarkdownFontFamilyProperty, value);
    }

    public FontWeight? MarkdownFontWeight
    {
        get => GetValue(MarkdownFontWeightProperty);
        set => SetValue(MarkdownFontWeightProperty, value);
    }

    public double MarkdownFontSize
    {
        get => GetValue(MarkdownFontSizeProperty);
        set => SetValue(MarkdownFontSizeProperty, value);
    }

    public double MarkdownLineHeight
    {
        get => GetValue(MarkdownLineHeightProperty);
        set => SetValue(MarkdownLineHeightProperty, value);
    }

    public double MarkdownLineSpacing
    {
        get => GetValue(MarkdownLineSpacingProperty);
        set => SetValue(MarkdownLineSpacingProperty, value);
    }

    public MarkdownBox()
    {
        SelectionEnabled = true;
        Cursor = TextCursor;
        Classes.Add("PixevalMarkdownBox");
        MarkdownStyle = new MarkdownStyleFluentTheme();
        Styles.Add(new MarkdownBoxStyles());
        Plugins.Plugins.Add(new PixevalMarkdownPlugin());
        Plugins.Plugins.Add(new SyntaxHighlight());
        Plugins.Plugins.Add(new PixevalHtmlPlugin());
        Plugins.Plugins.Add(new SvgFormat());
        Plugins.PathResolver = new PixevalPathResolver();
        Plugins.HyperlinkCommand = new RelayCommand<string>(OnHyperlinkClicked);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        QueueApplyMarkdownPresentation();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MarkdownProperty
            || change.Property == MarkdownForegroundProperty
            || change.Property == MarkdownFontFamilyProperty
            || change.Property == MarkdownFontWeightProperty
            || change.Property == MarkdownFontSizeProperty
            || change.Property == MarkdownLineHeightProperty
            || change.Property == MarkdownLineSpacingProperty)
        {
            QueueApplyMarkdownPresentation();
        }
    }

    protected override async void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.C
            && (e.KeyModifiers & KeyModifiers.Control) != 0
            && TryGetVisualSelectedText() is { Length: > 0 } selectedText
            && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            var item = new DataTransferItem();
            item.Set(DataFormat.Text, selectedText);
            var data = new DataTransfer();
            data.Add(item);

            await clipboard.SetDataAsync(data);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private async void OnHyperlinkClicked(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        var args = new MarkdownHyperlinkClickedEventArgs(url);
        HyperlinkClicked?.Invoke(this, args);
        if (args.Handled)
            return;

        if (TryScrollToAnchor(url))
            return;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return;

        if (TopLevel.GetTopLevel(this) is not
            {
                ViewContainer: { } viewContainer,
                Launcher: { } launcher
            })
            return;

        if (uri.Scheme is not AppInfo.AppProtocol)
            await launcher.LaunchUriAsync(uri);
        else if (uri.Host is "illust" && uri.AbsolutePath.Trim('/') is { } id)
            viewContainer.CreateIllustrationPage(id, IPlatformInfo.Pixiv);
    }

    private bool TryScrollToAnchor(string url)
    {
        if (!url.StartsWith('#'))
            return false;

        var anchor = Uri.UnescapeDataString(url[1..]);
        if (string.IsNullOrWhiteSpace(anchor))
            return false;

        var target = this
            .GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(control =>
                control.Classes.Contains(MarkdownAnchor.ClassName)
                && string.Equals(control.Tag as string, anchor, StringComparison.Ordinal));

        if (target?.TranslatePoint(default, this) is not { } point)
            return false;

        ScrollValue = new Vector(
            ScrollValue.X,
            Math.Max(0, ScrollValue.Y + point.Y - AnchorScrollOffset));

        return true;
    }

    private void QueueApplyMarkdownPresentation()
    {
        if (_presentationApplyQueued)
            return;

        _presentationApplyQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _presentationApplyQueued = false;
            ApplyMarkdownPresentation();
        }, DispatcherPriority.Loaded);
    }

    private string? TryGetVisualSelectedText()
    {
        var parts = new List<string>();
        foreach (var textBlock in this.GetVisualDescendants().OfType<CTextBlock>())
        {
            if (textBlock.Selection is not { } selection)
                continue;

            var text = textBlock.Text;
            var start = Math.Clamp(Math.Min(selection.From, selection.To), 0, text.Length);
            var end = Math.Clamp(Math.Max(selection.From, selection.To), 0, text.Length);
            if (end <= start)
                continue;

            parts.Add(text[start..end]);
        }

        return parts.Count switch
        {
            0 => null,
            1 => parts[0],
            _ => string.Join('\n', parts)
        };
    }

    private void ApplyMarkdownPresentation()
    {
        var fontFamily = MarkdownFontFamily;
        var fontWeight = MarkdownFontWeight;
        var hasFontSize = IsPositive(MarkdownFontSize);
        var hasLineHeight = IsPositive(MarkdownLineHeight);
        var hasLineSpacing = IsNonNegative(MarkdownLineSpacing);

        foreach (var text in this.GetVisualDescendants().OfType<CTextBlock>())
        {
            var isHeading = IsHeading(text);
            var isCodeBlock = IsCodeBlock(text);
            var hasNaturalLineHeightInline = ContainsNaturalLineHeightInline(text);

            NormalizeInlineDecorations(text);

            text.Cursor = TextCursor;

            if (isCodeBlock)
                continue;

            if (MarkdownForeground is { } foreground)
                text.Foreground = foreground;
            else
                text.ClearValue(CTextBlock.ForegroundProperty);

            if (fontFamily is not null)
                text.FontFamily = fontFamily;
            else
                text.ClearValue(CTextBlock.FontFamilyProperty);

            if (fontWeight is { } weight && !isHeading)
                text.FontWeight = weight;
            else
                text.ClearValue(CTextBlock.FontWeightProperty);

            if (hasFontSize && !isHeading)
                text.FontSize = MarkdownFontSize;
            else
                text.ClearValue(CTextBlock.FontSizeProperty);

            if (hasLineHeight && !isHeading && !hasNaturalLineHeightInline)
                text.LineHeight = MarkdownLineHeight;
            else
                text.ClearValue(CTextBlock.LineHeightProperty);

            if (hasLineSpacing && !isHeading)
                text.LineSpacing = MarkdownLineSpacing;
            else
                text.ClearValue(CTextBlock.LineSpacingProperty);
        }

        foreach (var text in this.GetVisualDescendants().OfType<TextBlock>())
        {
            text.Cursor = TextCursor;

            if (IsCodeBlock(text))
                continue;

            if (MarkdownForeground is { } foreground)
                text.Foreground = foreground;
            else
                text.ClearValue(TextBlock.ForegroundProperty);

            if (fontFamily is not null)
                text.FontFamily = fontFamily;
            else
                text.ClearValue(TextBlock.FontFamilyProperty);

            if (fontWeight is { } weight)
                text.FontWeight = weight;
            else
                text.ClearValue(TextBlock.FontWeightProperty);

            if (hasFontSize)
                text.FontSize = MarkdownFontSize;
            else
                text.ClearValue(TextBlock.FontSizeProperty);

            if (hasLineHeight)
                text.LineHeight = MarkdownLineHeight;
            else
                text.ClearValue(TextBlock.LineHeightProperty);
        }

        NormalizeCodeBlockToolbars();
    }

    private static bool IsHeading(CTextBlock text) =>
        text.Classes.Any(@class =>
            @class is "Heading1" or "Heading2" or "Heading3" or "Heading4" or "Heading5" or "Heading6");

    private static bool IsCodeBlock(StyledElement element) =>
        element.Classes.Contains(CodeBlockClass);

    private static bool ContainsNaturalLineHeightInline(CTextBlock text) =>
        text.Content.Any(ContainsNaturalLineHeightInline);

    private static bool ContainsNaturalLineHeightInline(CInline inline) =>
        inline is RubyInline
        || inline is CImage
        || (inline is CInlineUIContainer container && !container.Classes.Contains(InlineCodeSentinelClass))
        || (inline is CSpan { Content: { } content } && content.Any(ContainsNaturalLineHeightInline));

    private static void NormalizeInlineDecorations(CTextBlock text)
    {
        for (var i = 0; i < text.Content.Count; i++)
        {
            if (NormalizeInlineDecorations(text.Content[i], false))
            {
                var inline = text.Content[i];
                text.Content.RemoveAt(i);
                text.Content.Insert(i, inline);
            }
        }
    }

    private static bool NormalizeInlineDecorations(CInline inline, bool isInsideCode)
    {
        var changed = false;

        if (inline is CCode code)
        {
            changed |= EnsureInlineCodeSentinel(code);
        }
        else if (isInsideCode)
        {
            inline.Background = Brushes.Transparent;
        }

        if (inline is CSpan { Content: { } content })
        {
            var childIsInsideCode = isInsideCode || inline is CCode;
            foreach (var child in content)
                changed |= NormalizeInlineDecorations(child, childIsInsideCode);
        }

        return changed;
    }

    private static bool EnsureInlineCodeSentinel(CCode code)
    {
        if (code.Classes.Contains(InlineCodeNormalizedClass))
            return false;

        var content = code.Content.ToList();
        if (content.Count == 0)
            return false;

        content.Add(CreateInlineCodeSentinel());
        code.Content = content;
        code.Classes.Add(InlineCodeNormalizedClass);

        return true;
    }

    private static CInlineUIContainer CreateInlineCodeSentinel()
    {
        var sentinel = new CInlineUIContainer(new Border
        {
            Width = 0,
            Height = 1,
            IsHitTestVisible = false
        })
        {
            TextVerticalAlignment = TextVerticalAlignment.Center
        };
        sentinel.Classes.Add(InlineCodeSentinelClass);

        return sentinel;
    }

    private void NormalizeCodeBlockToolbars()
    {
        foreach (var border in this.GetVisualDescendants().OfType<Border>().Where(static border => border.Classes.Contains(CodeBlockClass)))
            NormalizeCodeBlockToolbar(border);
    }

    private static void NormalizeCodeBlockToolbar(Border codeBlock)
    {
        if (codeBlock.Child is Grid root && root.Classes.Contains(CodeBlockRootClass))
            return;

        if (codeBlock.Child is not Panel codePad
            || codePad.GetType().FullName is not CodePadTypeName
            || codePad.Children.OfType<TextEditor>().FirstOrDefault() is not { } textEditor
            || codePad.Children.OfType<Label>().FirstOrDefault(static label => label.Classes.Contains("LangInfo")) is not { } langLabel)
        {
            return;
        }

        _ = codePad.Children.Remove(textEditor);
        _ = codePad.Children.Remove(langLabel);

        langLabel.VerticalAlignment = VerticalAlignment.Center;

        var copyButton = new Button
        {
            Content = new SymbolIcon
            {
                Symbol = Symbol.Clipboard,
                FontSize = 14
            },
            VerticalAlignment = VerticalAlignment.Center
        };
        copyButton.Classes.Add("CopyButton");
        ToolTip.SetTip(copyButton, "复制代码");
        copyButton.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(textEditor)?.Clipboard is { } clipboard)
            {
                var item = new DataTransferItem();
                item.Set(DataFormat.Text, textEditor.Text ?? "");
                var data = new DataTransfer();
                data.Add(item);

                await clipboard.SetDataAsync(data);
            }
        };

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        toolbar.SetValue(Panel.ZIndexProperty, 1);
        toolbar.Classes.Add(CodeBlockToolbarClass);
        toolbar.Children.Add(langLabel);
        toolbar.Children.Add(copyButton);

        root = new Grid();
        root.Classes.Add(CodeBlockRootClass);

        root.Children.Add(textEditor);
        root.Children.Add(toolbar);

        codeBlock.Child = root;
    }

    private static bool IsPositive(double value) => !double.IsNaN(value) && value > 0;

    private static bool IsNonNegative(double value) => !double.IsNaN(value) && value >= 0;

    private sealed class PixevalPathResolver : IPathResolver
    {
        private readonly DefaultPathResolver _defaultPathResolver = new();

        public string? AssetPathRoot
        {
            get;
            set
            {
                field = value;
                _defaultPathResolver.AssetPathRoot = value;
            }
        }

        public IEnumerable<string>? CallerAssemblyNames
        {
            get;
            set
            {
                field = value;
                _defaultPathResolver.CallerAssemblyNames = value;
            }
        }

        public async Task<Stream?> ResolveImageResource(string relativeOrAbsolutePath)
        {
            if (Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var uri)
                && uri.Scheme is "http" or "https")
            {
                return await CacheHelper.GetImageStreamAsync(ResolvePlatform(uri), uri.OriginalString).ConfigureAwait(false);
            }

            return await (_defaultPathResolver.ResolveImageResource(relativeOrAbsolutePath) ?? Task.FromResult<Stream?>(null)).ConfigureAwait(false);
        }

        private static string ResolvePlatform(Uri uri)
        {
            var host = uri.Host;
            if (GitHubHttpOptions.IsGitHubHost(host))
                return GitHubHttpClientProvider.PlatformKey;
            if (IsHostOrSubdomain(host, "pixiv.net") || IsHostOrSubdomain(host, "pximg.net"))
                return IPlatformInfo.Pixiv;
            if (IsHostOrSubdomain(host, "donmai.us"))
                return IPlatformInfo.Danbooru;
            return IPlatformInfo.All;
        }

        private static bool IsHostOrSubdomain(string host, string domain) =>
            host.Equals(domain, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);
    }
}
