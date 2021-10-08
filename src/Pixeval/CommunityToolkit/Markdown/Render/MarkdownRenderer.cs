using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.CommunityToolkit.Markdown.Parsers.Core;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// Generates Framework Elements for the UWP Markdown TextBlock.
    /// </summary>
    public partial class MarkdownRenderer : MarkdownRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownRenderer"/> class.
        /// </summary>
        /// <param name="document">The Document to Render.</param>
        /// <param name="linkRegister">The LinkRegister, <see cref="MarkdownTextBlock"/> will use itself.</param>
        /// <param name="imageResolver">The Image Resolver, <see cref="MarkdownTextBlock"/> will use itself.</param>
        /// <param name="codeBlockResolver">The Code Block Resolver, <see cref="MarkdownTextBlock"/> will use itself.</param>
        public MarkdownRenderer(MarkdownDocument document, ILinkRegister linkRegister, IImageResolver imageResolver, ICodeBlockResolver codeBlockResolver)
            : base(document)
        {
            LinkRegister = linkRegister;
            ImageResolver = imageResolver;
            CodeBlockResolver = codeBlockResolver;
            DefaultEmojiFont = new FontFamily("Segoe UI Emoji");
        }

        /// <summary>
        /// Called externally to render markdown to a text block.
        /// </summary>
        /// <returns> A XAML UI element. </returns>
        public UIElement Render()
        {
            var stackPanel = new StackPanel();
            RootElement = stackPanel;
            Render(new UIElementCollectionRenderContext(stackPanel.Children) { Foreground = Foreground });

            // Set background and border properties.
            stackPanel.Background = Background;
            stackPanel.BorderBrush = BorderBrush;
            stackPanel.BorderThickness = BorderThickness;
            stackPanel.Padding = Padding;

            return stackPanel;
        }

        /// <summary>
        /// Creates a new RichTextBlock, if the last element of the provided collection isn't already a RichTextBlock.
        /// </summary>
        /// <returns>The rich text block</returns>
        protected RichTextBlock CreateOrReuseRichTextBlock(IRenderContext context)
        {
            if (context is not UIElementCollectionRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            // Reuse the last RichTextBlock, if possible.
            if (blockUIElementCollection != null && blockUIElementCollection.Count > 0 && blockUIElementCollection[^1] is RichTextBlock)
            {
                return (RichTextBlock) blockUIElementCollection[^1];
            }

            var result = new RichTextBlock
            {
                CharacterSpacing = CharacterSpacing,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStretch = FontStretch,
                FontStyle = FontStyle,
                FontWeight = FontWeight,
                Foreground = localContext.Foreground,
                IsTextSelectionEnabled = IsTextSelectionEnabled,
                TextWrapping = TextWrapping,
                FlowDirection = FlowDirection
            };
            localContext.BlockUIElementCollection?.Add(result);

            return result;
        }

        /// <summary>
        /// Creates a new TextBlock, with default settings.
        /// </summary>
        /// <returns>The created TextBlock</returns>
        protected TextBlock CreateTextBlock(RenderContext context)
        {
            var result = new TextBlock
            {
                CharacterSpacing = CharacterSpacing,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStretch = FontStretch,
                FontStyle = FontStyle,
                FontWeight = FontWeight,
                Foreground = context.Foreground,
                IsTextSelectionEnabled = IsTextSelectionEnabled,
                TextWrapping = TextWrapping,
                FlowDirection = FlowDirection
            };
            return result;
        }

        /// <summary>
        /// Performs an action against any runs that occur within the given span.
        /// </summary>
        protected void AlterChildRuns(Span parentSpan, Action<Span, Run> action)
        {
            foreach (var inlineElement in parentSpan.Inlines)
            {
                switch (inlineElement)
                {
                    case Span span:
                        AlterChildRuns(span, action);
                        break;
                    case Run run:
                        action(parentSpan, run);
                        break;
                }
            }
        }

        /// <summary>
        /// Checks if all text elements inside the given container are superscript.
        /// </summary>
        /// <returns> <c>true</c> if all text is superscript (level 1); <c>false</c> otherwise. </returns>
        private static bool AllTextIsSuperscript(IInlineContainer container, int superscriptLevel = 0)
        {
            foreach (var inline in container.Inlines!)
            {
                switch (inline)
                {
                    // Remove any nested superscripts.
                    case SuperscriptTextInline textInline when AllTextIsSuperscript(textInline, superscriptLevel + 1) == false:
                    // Remove any superscripts.
                    case IInlineContainer inlineContainer when AllTextIsSuperscript(inlineContainer, superscriptLevel) == false:
                    case IInlineLeaf leaf when !ParseHelpers.IsMarkdownBlankOrWhiteSpace(leaf.Text!) && superscriptLevel != 1:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes all superscript elements from the given container.
        /// </summary>
        private static void RemoveSuperscriptRuns(IInlineContainer container, bool insertCaret)
        {
            for (var i = 0; i < container.Inlines!.Count; i++)
            {
                var inline = container.Inlines[i];
                switch (inline)
                {
                    case SuperscriptTextInline textInline:
                    {
                        // Remove any nested superscripts.
                        RemoveSuperscriptRuns(textInline, insertCaret);

                        // Remove the superscript element, insert all the children.
                        container.Inlines.RemoveAt(i);
                        if (insertCaret)
                        {
                            container.Inlines.Insert(i++, new TextRunInline { Text = "^" });
                        }

                        foreach (var superscriptInline in textInline.Inlines!)
                        {
                            container.Inlines.Insert(i++, superscriptInline);
                        }

                        i--;
                        break;
                    }
                    case IInlineContainer inlineContainer:
                        // Remove any superscripts.
                        RemoveSuperscriptRuns(inlineContainer, insertCaret);
                        break;
                }
            }
        }

        private void Preventative_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pointerPoint = e.GetCurrentPoint((UIElement) sender);

            if (pointerPoint.Properties.IsHorizontalMouseWheel)
            {
                e.Handled = false;
                return;
            }

            var rootViewer = RootElement!.FindAscendant<ScrollViewer>();
            if (rootViewer != null)
            {
                pointerWheelChanged.Invoke(rootViewer, new object[] { e });
                e.Handled = true;
            }
        }
    }
}