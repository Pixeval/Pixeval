using System;
using System.Collections.Generic;
using System.Text;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render
{
    /// <summary>
    /// A base renderer for Rendering Markdown into Controls.
    /// </summary>
    public abstract partial class MarkdownRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownRendererBase"/> class.
        /// </summary>
        /// <param name="document">Markdown Document to Render</param>
        protected MarkdownRendererBase(MarkdownDocument document)
        {
            Document = document;
        }

        /// <summary>
        /// Renders all Content to the Provided Parent UI.
        /// </summary>
        /// <param name="context">UI Context</param>
        public virtual void Render(IRenderContext context)
        {
            RenderBlocks(Document.Blocks!, context);
        }

        /// <summary>
        /// Renders a list of block elements.
        /// </summary>
        protected virtual void RenderBlocks(IEnumerable<MarkdownBlock> blockElements, IRenderContext context)
        {
            foreach (var element in blockElements)
            {
                RenderBlock(element, context);
            }
        }

        /// <summary>
        /// Called to render a block element.
        /// </summary>
        protected void RenderBlock(MarkdownBlock element, IRenderContext context)
        {
            switch (element.Type)
            {
                case MarkdownBlockType.Paragraph:
                    RenderParagraph((ParagraphBlock)element, context);
                    break;

                case MarkdownBlockType.Quote:
                    RenderQuote((QuoteBlock)element, context);
                    break;

                case MarkdownBlockType.Code:
                    RenderCode((CodeBlock)element, context);
                    break;

                case MarkdownBlockType.Header:
                    RenderHeader((HeaderBlock)element, context);
                    break;

                case MarkdownBlockType.List:
                    RenderListElement((ListBlock)element, context);
                    break;

                case MarkdownBlockType.HorizontalRule:
                    RenderHorizontalRule(context);
                    break;

                case MarkdownBlockType.Table:
                    RenderTable((TableBlock)element, context);
                    break;

                case MarkdownBlockType.YamlHeader:
                    RenderYamlHeader((YamlHeaderBlock)element, context);
                    break;
                case MarkdownBlockType.Root:
                case MarkdownBlockType.ListItemBuilder:
                case MarkdownBlockType.LinkReference:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Renders all of the children for the given element.
        /// </summary>
        /// <param name="inlineElements"> The parsed inline elements to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected void RenderInlineChildren(IList<MarkdownInline> inlineElements, IRenderContext context)
        {
            foreach (var element in inlineElements)
            {
                switch (element.Type)
                {
                    case MarkdownInlineType.Comment:
                    case MarkdownInlineType.LinkReference:
                        break;

                    default:
                        RenderInline(element, context);
                        break;
                }
            }
        }

        /// <summary>
        /// Called to render an inline element.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected void RenderInline(MarkdownInline element, IRenderContext context)
        {
            switch (element.Type)
            {
                case MarkdownInlineType.TextRun:
                    RenderTextRun((TextRunInline) element, context);
                    break;

                case MarkdownInlineType.Italic:
                    RenderItalicRun((ItalicTextInline) element, context);
                    break;

                case MarkdownInlineType.Bold:
                    RenderBoldRun((BoldTextInline) element, context);
                    break;

                case MarkdownInlineType.MarkdownLink:
                    CheckRenderMarkdownLink((MarkdownLinkInline) element, context);
                    break;

                case MarkdownInlineType.RawHyperlink:
                    RenderHyperlink((HyperlinkInline) element, context);
                    break;

                case MarkdownInlineType.Strikethrough:
                    RenderStrikeThroughRun((StrikeThroughTextInline) element, context);
                    break;

                case MarkdownInlineType.Superscript:
                    RenderSuperscriptRun((SuperscriptTextInline) element, context);
                    break;

                case MarkdownInlineType.Subscript:
                    RenderSubscriptRun((SubscriptTextInline) element, context);
                    break;

                case MarkdownInlineType.Code:
                    RenderCodeRun((CodeInline) element, context);
                    break;

                case MarkdownInlineType.Image:
                    RenderImage((ImageInline) element, context);
                    break;

                case MarkdownInlineType.Emoji:
                    RenderEmoji((EmojiInline) element, context);
                    break;
            }
        }

        /// <summary>
        /// Removes leading whitespace, but only if this is the first run in the block.
        /// </summary>
        /// <returns>The corrected string</returns>
        protected string CollapseWhitespace(IRenderContext context, string text)
        {
            var noOutputWhitespace = context.TrimLeadingWhitespace;
            StringBuilder? result = null;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c is ' ' or '\t')
                {
                    if (noOutputWhitespace)
                    {
                        result ??= new StringBuilder(text[..i], text.Length);
                    }
                    else
                    {
                        result?.Append(c);

                        noOutputWhitespace = true;
                    }
                }
                else
                {
                    result?.Append(c);

                    noOutputWhitespace = false;
                }
            }

            context.TrimLeadingWhitespace = false;
            return result == null ? text : result.ToString();
        }

        /// <summary>
        /// Verifies if the link is valid, before processing into a link, or plain text.
        /// </summary>
        /// <param name="element"> The parsed inline element to render. </param>
        /// <param name="context"> Persistent state. </param>
        protected void CheckRenderMarkdownLink(MarkdownLinkInline element, IRenderContext context)
        {
            // Avoid processing when link text is empty.
            if (element.Inlines is { Count: 0 })
            {
                return;
            }

            // Attempt to resolve references.
            element.ResolveReference(Document);
            if (element.Url == null)
            {
                // The element couldn't be resolved, just render it as text.
                RenderInlineChildren(element.Inlines!, context);
                return;
            }

            foreach (var inline in element.Inlines!)
            {
                if (inline is ImageInline imageInline)
                {
                    // this is an image, create Image.
                    if (!string.IsNullOrEmpty(imageInline.ReferenceId))
                    {
                        imageInline.ResolveReference(Document);
                    }

                    imageInline.Url = element.Url;
                    RenderImage(imageInline, context);
                    return;
                }
            }

            RenderMarkdownLink(element, context);
        }

        /// <summary>
        /// Gets the markdown document that will be rendered.
        /// </summary>
        protected MarkdownDocument Document { get; }
    }
}