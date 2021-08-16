using HtmlAgilityPack;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Pixeval.Util.UI
{
    /// <summary>
    /// From https://github.com/xleon/HTML2XAML/blob/master/XAMLHtml/XAMLHtml.cs
    /// </summary>
    public sealed class HtmlProperties : DependencyObject
    {
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached(
                "Html",
                typeof(string),
                typeof(HtmlProperties),
                new PropertyMetadata(null, HtmlChanged));

        public static void SetHtml(DependencyObject obj, string value)
            => obj.SetValue(HtmlProperty, value);

        public static string GetHtml(DependencyObject obj)
            => (string) obj.GetValue(HtmlProperty);

        public static Func<Span>? H1SpanFactory { get; set; }
        public static Func<Span>? H2SpanFactory { get; set; }
        public static Func<Span>? H3SpanFactory { get; set; }

        public static Action<object, TappedRoutedEventArgs>? OnImageTapped { get; set; }

        public static double ImageMaxPixelWidth { get; set; } = 800.0;
        public static double ImageMaxPixelHeight { get; set; } = 600.0;

        private static RichTextBlock? _currentObject;

        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richText = d as RichTextBlock;
            if (richText == null) return;

            _currentObject = richText;

            //Generate blocks
            var xhtml = e.NewValue as string;
            var blocks = GenerateBlocksForHtml(xhtml!);

            _currentObject = null;

            //Add the blocks to the RichTextBlock
            richText.Blocks.Clear();
            foreach (var b in blocks)
            {
                richText.Blocks.Add(b);
            }
        }

        private static IEnumerable<Block> GenerateBlocksForHtml(string xhtml)
        {
            var blocks = new List<Block>();

            var doc = new HtmlDocument();
            doc.LoadHtml(xhtml);

            var block = GenerateParagraph(doc.DocumentNode);
            blocks.Add(block);
            return blocks;
        }

        // TODO this method seams to be removing necessary spaces in #text nodes
        private static string CleanText(string input)
        {
            var clean = ConvertToPlainText(input);
            if (clean == "\0")
            {
                clean = "\n";
            }
            return clean;
        }

        private static string ConvertToPlainText(string html)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(html);

            StringWriter sw = new();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (var subNode in node.ChildNodes)
            {
                ConvertTo(subNode, outText);
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    var parentName = node.ParentNode.Name;
                    if (parentName is "script" or "style")
                        break;

                    // get text
                    var html = ((HtmlTextNode)node).Text;
                    
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;
                    
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "br":
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        foreach (var subNode in node.ChildNodes)
                        {
                            ConvertContentTo(subNode, outText);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void AddChildren(Paragraph p, HtmlNode node)
        {
            var added = false;
            foreach (var child in node.ChildNodes)
            {
                var i = GenerateBlockForNode(child);
                if (i != null)
                {
                    p.Inlines.Add(i);
                    added = true;
                }
            }
            if (!added)
            {
                p.Inlines.Add(new Run { Text = CleanText(node.InnerText) });
            }
        }

        private static void AddChildren(Span s, HtmlNode node)
        {
            var added = false;

            foreach (var child in node.ChildNodes)
            {
                var i = GenerateBlockForNode(child);
                if (i != null)
                {
                    s.Inlines.Add(i);
                    added = true;
                }
            }
            if (!added)
            {
                s.Inlines.Add(new Run { Text = CleanText(node.InnerText) });
            }
        }

        private static Inline? GenerateBlockForNode(HtmlNode node)
        {
            switch (node.Name)
            {
                case "div":
                    return GenerateSpan(node);
                case "p":
                case "P":
                    return GenerateInnerParagraph(node);
                case "img":
                case "IMG":
                    return GenerateImage(node);
                case "a":
                case "A":
                    return node.ChildNodes.Count >= 1 && (node.FirstChild.Name == "img" || node.FirstChild.Name == "IMG")
                        ? GenerateImage(node.FirstChild)
                        : GenerateHyperLink(node);
                case "li":
                case "LI":
                    return GenerateLi(node);
                case "b":
                case "B":
                case "strong":
                case "STRONG":
                    return GenerateBold(node);
                case "i":
                case "I":
                case "em":
                case "EM":
                    return GenerateItalic(node);
                case "u":
                case "U":
                    return GenerateUnderline(node);
                case "br":
                case "BR":
                    return new LineBreak();
                case "span":
                case "Span":
                    return GenerateSpan(node);
                case "iframe":
                case "Iframe":
                    return GenerateIFrame(node);
                case "#text":
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                        //return new Run { Text = CleanText(node.InnerText) }; // CleanText is removing white spaces in this case
                        return new Run { Text = node.InnerText };
                    break;
                case "h1":
                case "H1":
                    return GenerateH1(node);
                case "h2":
                case "H2":
                    return GenerateH2(node);
                case "h3":
                case "H3":
                    return GenerateH3(node);
                case "ul":
                case "UL":
                    return GenerateUl(node);
                default:
                    return GenerateSpanWNewLine(node);
            }
            return null;
        }

        private static Inline GenerateLi(HtmlNode node)
        {
            var span = new Span();
            var inlineUiContainer = new InlineUIContainer();
            var ellipse = new Ellipse
            {
                Fill = _currentObject!.Foreground ?? new SolidColorBrush(Colors.Black),
                Width = 6,
                Height = 6,
                Margin = new Thickness(-30, 0, 0, 1)
            };
            inlineUiContainer.Child = ellipse;
            span.Inlines.Add(inlineUiContainer);
            AddChildren(span, node);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateImage(HtmlNode node)
        {
            var span = new Span();
            try
            {
                var inlineUiContainer = new InlineUIContainer();
                var sourceUri = WebUtility.HtmlDecode(node.Attributes["src"].Value);
                var sourceWidth = WebUtility.HtmlDecode(node.Attributes["width"]?.Value);
                var sourceHeight = WebUtility.HtmlDecode(node.Attributes["height"]?.Value);

                var image = new Image
                {
                    Stretch = Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                if (sourceWidth != null || sourceHeight != null)
                {
                    if (sourceWidth != null)
                    {
                        image.MaxWidth = double.Parse(sourceWidth);
                        image.Width = double.Parse(sourceWidth);
                    }

                    if (sourceHeight != null)
                    {
                        image.MaxHeight = double.Parse(sourceHeight);
                        image.Height = double.Parse(sourceHeight);
                    }
                }
                else
                {
                    image.ImageOpened += ImageOpened;
                }

                image.ImageFailed += ImageFailed;
                image.Tapped += ImageOnTapped;

                image.Source = new BitmapImage(new Uri(sourceUri, UriKind.Absolute));

                inlineUiContainer.Child = image;

                span.Inlines.Add(inlineUiContainer);
                span.Inlines.Add(new LineBreak());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return span;
        }

        private static void ImageOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
            => OnImageTapped?.Invoke(sender, tappedRoutedEventArgs);

        private static void ImageFailed(object sender, ExceptionRoutedEventArgs e)
            => Debug.WriteLine("image failed to load");

        private static void ImageOpened(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;

            var bitmapImg = img.Source as BitmapImage;
            if (bitmapImg != null && (bitmapImg.PixelWidth > ImageMaxPixelWidth || bitmapImg.PixelHeight > ImageMaxPixelHeight))
            {
                img.Width = ImageMaxPixelWidth;
                img.Height = ImageMaxPixelHeight;

                if (bitmapImg.PixelWidth > ImageMaxPixelWidth)
                {
                    img.Width = ImageMaxPixelWidth;
                    img.Height = (ImageMaxPixelWidth / bitmapImg.PixelWidth) * bitmapImg.PixelHeight;
                }
                if (img.Height > ImageMaxPixelHeight)
                {
                    img.Height = ImageMaxPixelHeight;
                    img.Width = (ImageMaxPixelHeight / img.Height) * img.Width;
                }
            }
            else
            {
                if (bitmapImg == null) return;

                img.Height = bitmapImg.PixelHeight;
                img.Width = bitmapImg.PixelWidth;
            }
        }

        private static Inline GenerateHyperLink(HtmlNode node)
        {
            var span = new Span();
            var inlineUiContainer = new InlineUIContainer();
            var hyperlinkButton = new HyperlinkButton
            {
                NavigateUri = new Uri(node.Attributes["href"].Value, UriKind.Absolute),
                Content = CleanText(node.InnerText)
            };

            inlineUiContainer.Child = hyperlinkButton;
            span.Inlines.Add(inlineUiContainer);
            return span;
        }

        private static Inline? GenerateIFrame(HtmlNode node)
        {
            try
            {
                var span = new Span();
                span.Inlines.Add(new LineBreak());
                var inlineUiContainer = new InlineUIContainer();
                var webView = new WebView2
                {
                    Source = new Uri(node.Attributes["src"].Value, UriKind.Absolute),
                    Width = int.Parse(node.Attributes["width"].Value),
                    Height = int.Parse(node.Attributes["height"].Value)
                };
                inlineUiContainer.Child = webView;
                span.Inlines.Add(inlineUiContainer);
                span.Inlines.Add(new LineBreak());
                return span;
            }
            catch
            {
                return null;
            }
        }

        private static Inline GenerateBold(HtmlNode node)
        {
            var bold = new Bold();
            AddChildren(bold, node);
            return bold;
        }

        private static Inline GenerateUnderline(HtmlNode node)
        {
            var underline = new Underline();
            AddChildren(underline, node);
            return underline;
        }

        private static Inline GenerateItalic(HtmlNode node)
        {
            var italic = new Italic();
            AddChildren(italic, node);
            return italic;
        }

        private static Block GenerateParagraph(HtmlNode node)
        {
            var paragraph = new Paragraph();
            AddChildren(paragraph, node);
            return paragraph;
        }

        private static Inline GenerateUl(HtmlNode node)
        {
            var span = new Span();
            span.Inlines.Add(new LineBreak());
            AddChildren(span, node);
            return span;
        }

        private static Inline GenerateInnerParagraph(HtmlNode node)
        {
            var span = new Span();
            span.Inlines.Add(new LineBreak());
            AddChildren(span, node);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateSpan(HtmlNode node)
        {
            var span = new Span();
            AddChildren(span, node);
            return span;
        }

        private static Inline GenerateSpanWNewLine(HtmlNode node)
        {
            var span = new Span();
            AddChildren(span, node);
            if (span.Inlines.Count > 0)
                span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Span GenerateH3(HtmlNode node)
        {
            var span = H3SpanFactory?.Invoke() ?? new Span();
            span.Inlines.Add(new LineBreak());
            var bold = new Bold();
            var run = new Run { Text = CleanText(node.InnerText) };
            bold.Inlines.Add(run);
            span.Inlines.Add(bold);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateH2(HtmlNode node)
        {
            var span = H2SpanFactory?.Invoke() ?? new Span { FontSize = 24 };
            span.Inlines.Add(new LineBreak());
            var run = new Run { Text = CleanText(node.InnerText) };
            span.Inlines.Add(run);
            span.Inlines.Add(new LineBreak());
            return span;
        }

        private static Inline GenerateH1(HtmlNode node)
        {
            var span = H1SpanFactory?.Invoke() ?? new Span { FontSize = 30 };
            span.Inlines.Add(new LineBreak());
            var run = new Run { Text = CleanText(node.InnerText) };
            span.Inlines.Add(run);
            span.Inlines.Add(new LineBreak());
            return span;
        }
    }
}