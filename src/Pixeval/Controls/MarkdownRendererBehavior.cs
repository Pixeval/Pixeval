// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using Avalonia;
using LiveMarkdown.Avalonia;

namespace Pixeval.Controls;

/// <summary>
/// Provides attached properties for incremental loading behavior on ItemsControl.
/// </summary>
public static class MarkdownRendererBehavior
{
    /// <summary>
    /// Attached property to set text.
    /// </summary>
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<MarkdownRenderer, string>(
            "Text",
            typeof(IncrementalLoadingBehavior),
            defaultValue: "");

    extension(MarkdownRenderer markdownRenderer)
    {
        public string Text
        {
            get => markdownRenderer.GetValue(TextProperty);
            set => markdownRenderer.SetValue(TextProperty, value);
        }
    }

    // Static accessors for XAML
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetText(MarkdownRenderer element) => element.GetValue(TextProperty);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetText(MarkdownRenderer element, string value) => element.SetValue(TextProperty, value);

    static MarkdownRendererBehavior()
    {
        TextProperty.Changed.AddClassHandler<MarkdownRenderer>(OnTextPropertyChanged);
    }

    private static void OnTextPropertyChanged(MarkdownRenderer sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.MarkdownBuilder = new ObservableStringBuilder().Append(e.GetNewValue<string>()
            .Replace("<br />", "\n\n")
            .Replace("<br >", "\n\n")
            .Replace("<br/>", "\n\n")
            .Replace("<br>", "\n\n"));
    }
}
