#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownRenderer.Inlines.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using Windows.UI.Text;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     Inline UI Methods for UWP UI Creation.
    /// </summary>
    public partial class MarkdownRenderer
    {
        /// <summary>
        ///     Renders emoji element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderEmoji(EmojiInline element, IRenderContext context)
        {
            if (context is not InlineRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var inlineCollection = localContext.InlineCollection;

            var emoji = new Run
            {
                FontFamily = EmojiFontFamily ?? DefaultEmojiFont,
                Text = element.Text
            };

            inlineCollection.Add(emoji);
        }

        /// <summary>
        ///     Renders a text run element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderTextRun(TextRunInline element, IRenderContext context)
        {
            InternalRenderTextRun(element, context);
        }

        private Run InternalRenderTextRun(IInlineLeaf element, IRenderContext context)
        {
            if (context is not InlineRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var inlineCollection = localContext.InlineCollection;

            // Create the text run
            var textRun = new Run
            {
                Text = CollapseWhitespace(context, element.Text!)
            };

            // Add it
            inlineCollection.Add(textRun);
            return textRun;
        }

        /// <summary>
        ///     Renders a bold run element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderBoldRun(BoldTextInline element, IRenderContext context)
        {
            if (context is not InlineRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            // Create the text run
            var boldSpan = new Span
            {
                FontWeight = FontWeights.Bold
            };

            var childContext = new InlineRenderContext(boldSpan.Inlines, context)
            {
                Parent = boldSpan,
                WithinBold = true
            };

            // Render the children into the bold inline.
            RenderInlineChildren(element.Inlines!, childContext);

            // Add it to the current inlines
            localContext.InlineCollection.Add(boldSpan);
        }

        /// <summary>
        ///     Renders a link element
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderMarkdownLink(MarkdownLinkInline element, IRenderContext context)
        {
            if (context is not InlineRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            // HACK: Superscript is not allowed within a hyperlink.  But if we switch it around, so
            // that the superscript is outside the hyperlink, then it will render correctly.
            // This assumes that the entire hyperlink is to be rendered as superscript.
            if (AllTextIsSuperscript(element) == false)
            {
                // Regular ol' hyperlink.
                var link = new Hyperlink();

                // Register the link
                LinkRegister.RegisterNewHyperLink(link, element.Url!);

                // Remove superscripts.
                RemoveSuperscriptRuns(element, true);

                // Render the children into the link inline.
                var childContext = new InlineRenderContext(link.Inlines, context)
                {
                    Parent = link,
                    WithinHyperlink = true
                };

                if (localContext.OverrideForeground)
                {
                    link.Foreground = localContext.Foreground;
                }
                else if (LinkForeground != null)
                {
                    link.Foreground = LinkForeground;
                }

                RenderInlineChildren(element.Inlines!, childContext);
                context.TrimLeadingWhitespace = childContext.TrimLeadingWhitespace;

                ToolTipService.SetToolTip(link, element.Tooltip ?? element.Url);

                // Add it to the current inlines
                localContext.InlineCollection.Add(link);
            }
            else
            {
                // THE HACK IS ON!

                // Create a fake superscript element.
                var fakeSuperscript = new SuperscriptTextInline
                {
                    Inlines = new List<MarkdownInline>
                    {
                        element
                    }
                };

                // Remove superscripts.
                RemoveSuperscriptRuns(element, false);

                // Now render it.
                RenderSuperscriptRun(fakeSuperscript, context);
            }
        }

        /// <summary>
        ///     Renders a raw link element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderHyperlink(HyperlinkInline element, IRenderContext context)
        {
            if (context is not InlineRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var link = new Hyperlink();

            // Register the link
            LinkRegister.RegisterNewHyperLink(link, element.Url!);

            var brush = localContext.Foreground;
            if (LinkForeground != null && !localContext.OverrideForeground)
            {
                brush = LinkForeground;
            }

            // Make a text block for the link
            var linkText = new Run
            {
                Text = CollapseWhitespace(context, element.Text!),
                Foreground = brush
            };

            link.Inlines.Add(linkText);

            // Add it to the current inlines
            localContext.InlineCollection.Add(link);
        }

        /// <summary>
        ///     Renders an image element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override async void RenderImage(ImageInline element, IRenderContext context)
        {
            if (context is not InlineRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var inlineCollection = localContext.InlineCollection;

            var placeholder = InternalRenderTextRun(new TextRunInline { Text = element.Text, Type = MarkdownInlineType.TextRun }, context);
            var resolvedImage = await ImageResolver.ResolveImageAsync(element.RenderUrl!, element.Tooltip!);

            // if image can not be resolved we have to return
            if (resolvedImage == null)
            {
                return;
            }

            var image = new Image
            {
                Source = resolvedImage,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Stretch = ImageStretch
            };

            var hyperlinkButton = new HyperlinkButton
            {
                Content = image
            };

            var viewBox = new Viewbox
            {
                Child = hyperlinkButton,
                StretchDirection = StretchDirection.DownOnly
            };

            viewBox.PointerWheelChanged += Preventative_PointerWheelChanged;

            var scrollViewer = new ScrollViewer
            {
                Content = viewBox,
                VerticalScrollMode = ScrollMode.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            var imageContainer = new InlineUIContainer { Child = scrollViewer };

            LinkRegister.RegisterNewHyperLink(image, element.Url!, element.RenderUrl != element.Url);

            if (ImageMaxHeight > 0)
            {
                viewBox.MaxHeight = ImageMaxHeight;
            }

            if (ImageMaxWidth > 0)
            {
                viewBox.MaxWidth = ImageMaxWidth;
            }

            if (element.ImageWidth > 0)
            {
                image.Width = element.ImageWidth;
                image.Stretch = Stretch.UniformToFill;
            }

            if (element.ImageHeight > 0)
            {
                if (element.ImageWidth == 0)
                {
                    image.Width = element.ImageHeight;
                }

                image.Height = element.ImageHeight;
                image.Stretch = Stretch.UniformToFill;
            }

            if (element.ImageHeight > 0 && element.ImageWidth > 0)
            {
                image.Stretch = Stretch.Fill;
            }

            // If image size is given then scroll to view overflown part
            if (element.ImageHeight > 0 || element.ImageWidth > 0)
            {
                scrollViewer.HorizontalScrollMode = ScrollMode.Auto;
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }

            // Else resize the image
            else
            {
                scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }

            ToolTipService.SetToolTip(image, element.Tooltip);

            // Try to add it to the current inlines
            // Could fail because some containers like Hyperlink cannot have inlined images
            try
            {
                var placeholderIndex = inlineCollection.IndexOf(placeholder);
                inlineCollection.Remove(placeholder);
                inlineCollection.Insert(placeholderIndex, imageContainer);
            }
            catch
            {
                // Ignore error
            }
        }

        /// <summary>
        ///     Renders a text run element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderItalicRun(ItalicTextInline element, IRenderContext context)
        {
            var localContext = context as InlineRenderContext;
            if (localContext == null)
            {
                throw new RenderContextIncorrectException();
            }

            // Create the text run
            var italicSpan = new Span
            {
                FontStyle = FontStyle.Italic
            };

            var childContext = new InlineRenderContext(italicSpan.Inlines, context)
            {
                Parent = italicSpan,
                WithinItalics = true
            };

            // Render the children into the italic inline.
            RenderInlineChildren(element.Inlines!, childContext);

            // Add it to the current inlines
            localContext.InlineCollection.Add(italicSpan);
        }

        /// <summary>
        ///     Renders a strike through element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderStrikeThroughRun(StrikeThroughTextInline element, IRenderContext context)
        {
            if (context is not InlineRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var span = new Span
            {
                TextDecorations = TextDecorations.Strikethrough
            };

            var childContext = new InlineRenderContext(span.Inlines, context)
            {
                Parent = span
            };

            // Render the children into the inline.
            RenderInlineChildren(element.Inlines!, childContext);

            // Add it to the current inlines
            localContext.InlineCollection.Add(span);
        }

        /// <summary>
        ///     Renders a superscript element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderSuperscriptRun(SuperscriptTextInline element, IRenderContext context)
        {
            var localContext = context as InlineRenderContext;
            var parent = localContext?.Parent as TextElement;
            if (localContext == null && parent == null)
            {
                throw new RenderContextIncorrectException();
            }

            // Le <sigh>, InlineUIContainers are not allowed within hyperlinks.
            if (localContext!.WithinHyperlink)
            {
                RenderInlineChildren(element.Inlines!, context);
                return;
            }

            var paragraph = new Paragraph
            {
                FontSize = parent!.FontSize * 0.8,
                FontFamily = parent.FontFamily,
                FontStyle = parent.FontStyle,
                FontWeight = parent.FontWeight
            };

            var childContext = new InlineRenderContext(paragraph.Inlines, context)
            {
                Parent = paragraph
            };

            RenderInlineChildren(element.Inlines!, childContext);

            var richTextBlock = CreateOrReuseRichTextBlock(new UIElementCollectionRenderContext(null, context));
            richTextBlock.Blocks.Add(paragraph);

            var border = new Border
            {
                Padding = new Thickness(0, 0, 0, paragraph.FontSize * 0.2),
                Child = richTextBlock
            };

            var inlineUIContainer = new InlineUIContainer
            {
                Child = border
            };

            // Add it to the current inlines
            localContext.InlineCollection.Add(inlineUIContainer);
        }

        /// <summary>
        ///     Renders a subscript element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderSubscriptRun(SubscriptTextInline element, IRenderContext context)
        {
            var localContext = context as InlineRenderContext;
            var parent = localContext?.Parent as TextElement;
            if (localContext == null && parent == null)
            {
                throw new RenderContextIncorrectException();
            }

            var paragraph = new Paragraph
            {
                FontSize = parent!.FontSize * 0.7,
                FontFamily = parent.FontFamily,
                FontStyle = parent.FontStyle,
                FontWeight = parent.FontWeight
            };

            var childContext = new InlineRenderContext(paragraph.Inlines, context)
            {
                Parent = paragraph
            };

            RenderInlineChildren(element.Inlines!, childContext);

            var richTextBlock = CreateOrReuseRichTextBlock(new UIElementCollectionRenderContext(null, context));
            richTextBlock.Blocks.Add(paragraph);

            var border = new Border
            {
                Margin = new Thickness(0, 0, 0, -1 * (paragraph.FontSize * 0.6)),
                Child = richTextBlock
            };

            var inlineUIContainer = new InlineUIContainer
            {
                Child = border
            };

            // Add it to the current inlines
            localContext!.InlineCollection.Add(inlineUIContainer);
        }

        /// <summary>
        ///     Renders a code element
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected override void RenderCodeRun(CodeInline element, IRenderContext context)
        {
            var localContext = context as InlineRenderContext;
            if (localContext == null)
            {
                throw new RenderContextIncorrectException();
            }

            var text = CollapseWhitespace(context, element.Text!);

            // Avoid a crash if the current inline is inside an hyper line.
            // This happens when using inline code blocks like [`SomeCode`](https://www.foo.bar).
            if (localContext.Parent is Hyperlink)
            {
                // Fallback span
                var run = new Run
                {
                    Text = text,
                    FontFamily = InlineCodeFontFamily ?? FontFamily,
                    Foreground = InlineCodeForeground ?? Foreground
                };

                // Additional formatting
                if (localContext.WithinItalics)
                {
                    run.FontStyle = FontStyle.Italic;
                }

                if (localContext.WithinBold)
                {
                    run.FontWeight = FontWeights.Bold;
                }

                // Add the fallback block
                localContext.InlineCollection.Add(run);
            }
            else
            {
                var textBlock = CreateTextBlock(localContext);
                textBlock.Text = text;
                textBlock.FontFamily = InlineCodeFontFamily ?? FontFamily;
                textBlock.Foreground = InlineCodeForeground ?? Foreground;

                if (localContext.WithinItalics)
                {
                    textBlock.FontStyle = FontStyle.Italic;
                }

                if (localContext.WithinBold)
                {
                    textBlock.FontWeight = FontWeights.Bold;
                }

                var inlineUIContainer = new InlineUIContainer
                {
                    Child = new Border
                    {
                        BorderThickness = InlineCodeBorderThickness,
                        BorderBrush = InlineCodeBorderBrush,
                        Background = InlineCodeBackground,
                        Child = textBlock,
                        Padding = InlineCodePadding,
                        Margin = InlineCodeMargin,

                        // Aligns content in InlineUI, see https://social.msdn.microsoft.com/Forums/silverlight/en-US/48b5e91e-efc5-4768-8eaf-f897849fcf0b/richtextbox-inlineuicontainer-vertical-alignment-issue?forum=silverlightarchieve
                        RenderTransform = new TranslateTransform { Y = 4 }
                    }
                };

                // Add it to the current inlines
                localContext.InlineCollection.Add(inlineUIContainer);
            }
        }
    }
}