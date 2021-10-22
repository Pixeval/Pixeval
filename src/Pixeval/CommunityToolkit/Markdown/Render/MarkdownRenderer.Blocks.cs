#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownRenderer.Blocks.cs
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

using System;
using System.Collections.Generic;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Shapes;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     Block UI Methods for UWP UI Creation.
    /// </summary>
    public partial class MarkdownRenderer
    {
        /// <summary>
        ///     Renders a list of block elements.
        /// </summary>
        protected override void RenderBlocks(IEnumerable<MarkdownBlock> blockElements, IRenderContext context)
        {
            if (context is not UIElementCollectionRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            base.RenderBlocks(blockElements, context);

            // Remove the top margin from the first block element, the bottom margin from the last block element,
            // and collapse adjacent margins.
            FrameworkElement? previousFrameworkElement = null;
            for (var i = 0; i < blockUIElementCollection!.Count; i++)
            {
                var frameworkElement = blockUIElementCollection[i] as FrameworkElement;
                if (frameworkElement != null)
                {
                    if (i == 0)
                    {
                        // Remove the top margin.
                        frameworkElement.Margin = new Thickness(
                            frameworkElement.Margin.Left,
                            0,
                            frameworkElement.Margin.Right,
                            frameworkElement.Margin.Bottom);
                    }
                    else if (previousFrameworkElement != null)
                    {
                        // Remove the bottom margin.
                        frameworkElement.Margin = new Thickness(
                            frameworkElement.Margin.Left,
                            Math.Max(frameworkElement.Margin.Top, previousFrameworkElement.Margin.Bottom),
                            frameworkElement.Margin.Right,
                            frameworkElement.Margin.Bottom);
                        previousFrameworkElement.Margin = new Thickness(
                            previousFrameworkElement.Margin.Left,
                            previousFrameworkElement.Margin.Top,
                            previousFrameworkElement.Margin.Right,
                            0);
                    }
                }

                previousFrameworkElement = frameworkElement;
            }
        }

        /// <summary>
        ///     Renders a paragraph element.
        /// </summary>
        protected override void RenderParagraph(ParagraphBlock element, IRenderContext context)
        {
            var paragraph = new Paragraph
            {
                Margin = ParagraphMargin,
                LineHeight = ParagraphLineHeight
            };

            var childContext = new InlineRenderContext(paragraph.Inlines, context)
            {
                Parent = paragraph
            };

            RenderInlineChildren(element.Inlines!, childContext);

            var textBlock = CreateOrReuseRichTextBlock(context);
            textBlock.Blocks.Add(paragraph);
        }

        /// <summary>
        ///     Renders a yaml header element.
        /// </summary>
        protected override void RenderYamlHeader(YamlHeaderBlock element, IRenderContext context)
        {
            var localContext = context as UIElementCollectionRenderContext;
            if (localContext == null)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            var table = new MarkdownTable(element.Children!.Count, 2, YamlBorderThickness, YamlBorderBrush!)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = TableMargin
            };

            // Split key and value
            var childrenKeys = new string[element.Children.Count];
            var childrenValues = new string[element.Children.Count];
            element.Children.Keys.CopyTo(childrenKeys, 0);
            element.Children.Values.CopyTo(childrenValues, 0);

            // Add each column
            for (var i = 0; i < element.Children.Count; i++)
            {
                // Add each cell
                var keyCell = new TextBlock
                {
                    Text = childrenKeys[i],
                    Foreground = Foreground,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Margin = TableCellPadding
                };
                var valueCell = new TextBlock
                {
                    Text = childrenValues[i],
                    Foreground = Foreground,
                    TextAlignment = TextAlignment.Left,
                    Margin = TableCellPadding,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(keyCell, 0);
                Grid.SetColumn(keyCell, i);
                Grid.SetRow(valueCell, 1);
                Grid.SetColumn(valueCell, i);
                table.Children.Add(keyCell);
                table.Children.Add(valueCell);
            }

            blockUIElementCollection?.Add(table);
        }

        /// <summary>
        ///     Renders a header element.
        /// </summary>
        protected override void RenderHeader(HeaderBlock element, IRenderContext context)
        {
            var textBlock = CreateOrReuseRichTextBlock(context);

            var paragraph = new Paragraph();
            var childInlines = paragraph.Inlines;
            switch (element.HeaderLevel)
            {
                case 1:
                    paragraph.Margin = Header1Margin;
                    paragraph.FontSize = Header1FontSize;
                    paragraph.FontWeight = Header1FontWeight;
                    paragraph.Foreground = Header1Foreground;
                    break;

                case 2:
                    paragraph.Margin = Header2Margin;
                    paragraph.FontSize = Header2FontSize;
                    paragraph.FontWeight = Header2FontWeight;
                    paragraph.Foreground = Header2Foreground;
                    break;

                case 3:
                    paragraph.Margin = Header3Margin;
                    paragraph.FontSize = Header3FontSize;
                    paragraph.FontWeight = Header3FontWeight;
                    paragraph.Foreground = Header3Foreground;
                    break;

                case 4:
                    paragraph.Margin = Header4Margin;
                    paragraph.FontSize = Header4FontSize;
                    paragraph.FontWeight = Header4FontWeight;
                    paragraph.Foreground = Header4Foreground;
                    break;

                case 5:
                    paragraph.Margin = Header5Margin;
                    paragraph.FontSize = Header5FontSize;
                    paragraph.FontWeight = Header5FontWeight;
                    paragraph.Foreground = Header5Foreground;
                    break;

                case 6:
                    paragraph.Margin = Header6Margin;
                    paragraph.FontSize = Header6FontSize;
                    paragraph.FontWeight = Header6FontWeight;
                    paragraph.Foreground = Header6Foreground;

                    var underline = new Underline();
                    childInlines = underline.Inlines;
                    paragraph.Inlines.Add(underline);
                    break;
            }

            // Render the children into the para inline.
            var childContext = new InlineRenderContext(childInlines, context)
            {
                Parent = paragraph,
                TrimLeadingWhitespace = true
            };

            RenderInlineChildren(element.Inlines!, childContext);

            // Add it to the blocks
            textBlock.Blocks.Add(paragraph);
        }

        /// <summary>
        ///     Renders a list element.
        /// </summary>
        protected override void RenderListElement(ListBlock element, IRenderContext context)
        {
            var localContext = context as UIElementCollectionRenderContext;
            if (localContext == null)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            // Create a grid with two columns.
            var grid = new Grid
            {
                Margin = ListMargin
            };

            // The first column for the bullet (or number) and the second for the text.
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ListGutterWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (var rowIndex = 0; rowIndex < element.Items!.Count; rowIndex++)
            {
                var listItem = element.Items[rowIndex];

                // Add a row definition.
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Add the bullet or number.
                var bullet = CreateTextBlock(localContext);
                bullet.Margin = ParagraphMargin;
                switch (element.Style)
                {
                    case ListStyle.Bulleted:
                        bullet.Text = "•";
                        break;

                    case ListStyle.Numbered:
                        bullet.Text = $"{rowIndex + 1}.";
                        break;
                }

                bullet.HorizontalAlignment = HorizontalAlignment.Right;
                bullet.Margin = new Thickness(0, 0, ListBulletSpacing, 0);
                Grid.SetRow(bullet, rowIndex);
                grid.Children.Add(bullet);

                // Add the list item content.
                var content = new StackPanel();
                var childContext = new UIElementCollectionRenderContext(content.Children, localContext);
                RenderBlocks(listItem!.Blocks!, childContext);
                Grid.SetColumn(content, 1);
                Grid.SetRow(content, rowIndex);
                grid.Children.Add(content);
            }

            blockUIElementCollection?.Add(grid);
        }

        /// <summary>
        ///     Renders a horizontal rule element.
        /// </summary>
        protected override void RenderHorizontalRule(IRenderContext context)
        {
            if (context is not UIElementCollectionRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            var brush = localContext.Foreground;
            if (HorizontalRuleBrush != null && !localContext.OverrideForeground)
            {
                brush = HorizontalRuleBrush;
            }

            var rectangle = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = HorizontalRuleThickness,
                Fill = brush,
                Margin = HorizontalRuleMargin
            };

            blockUIElementCollection?.Add(rectangle);
        }

        /// <summary>
        ///     Renders a quote element.
        /// </summary>
        protected override void RenderQuote(QuoteBlock element, IRenderContext context)
        {
            var localContext = context as UIElementCollectionRenderContext;
            if (localContext == null)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            var stackPanel = new StackPanel();
            var childContext = new UIElementCollectionRenderContext(stackPanel.Children, context)
            {
                Parent = stackPanel
            };

            if (QuoteForeground != null && !localContext.OverrideForeground)
            {
                childContext.Foreground = QuoteForeground;
            }

            RenderBlocks(element.Blocks!, childContext);

            var border = new Border
            {
                Margin = QuoteMargin,
                Background = QuoteBackground,
                BorderBrush = childContext.OverrideForeground ? childContext.Foreground : QuoteBorderBrush ?? childContext.Foreground,
                BorderThickness = QuoteBorderThickness,
                Padding = QuotePadding,
                Child = stackPanel
            };

            blockUIElementCollection?.Add(border);
        }

        /// <summary>
        ///     Renders a code element.
        /// </summary>
        protected override void RenderCode(CodeBlock element, IRenderContext context)
        {
            if (context is not UIElementCollectionRenderContext localContext)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            var brush = localContext.Foreground;
            if (CodeForeground != null && !localContext.OverrideForeground)
            {
                brush = CodeForeground;
            }

            var textBlock = new RichTextBlock
            {
                FontFamily = CodeFontFamily ?? FontFamily,
                Foreground = brush,
                LineHeight = FontSize * 1.4,
                FlowDirection = FlowDirection
            };

            textBlock.PointerWheelChanged += Preventative_PointerWheelChanged;

            var paragraph = new Paragraph();
            textBlock.Blocks.Add(paragraph);

            // Allows external Syntax Highlighting
            var hasCustomSyntax = CodeBlockResolver.ParseSyntax(paragraph.Inlines, element.Text!, element.CodeLanguage!);
            if (!hasCustomSyntax)
            {
                paragraph.Inlines.Add(new Run { Text = element.Text });
            }

            // Ensures that Code has Horizontal Scroll and doesn't wrap.
            var viewer = new ScrollViewer
            {
                Background = CodeBackground,
                BorderBrush = CodeBorderBrush,
                BorderThickness = CodeBorderThickness,
                Padding = CodePadding,
                Margin = CodeMargin,
                Content = textBlock
            };

            if (!WrapCodeBlock)
            {
                viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                viewer.HorizontalScrollMode = ScrollMode.Auto;
                viewer.VerticalScrollMode = ScrollMode.Disabled;
                viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }

            // Add it to the blocks
            blockUIElementCollection?.Add(viewer);
        }

        /// <summary>
        ///     Renders a table element.
        /// </summary>
        protected override void RenderTable(TableBlock element, IRenderContext context)
        {
            var localContext = context as UIElementCollectionRenderContext;
            if (localContext == null)
            {
                throw new RenderContextIncorrectException();
            }

            var blockUIElementCollection = localContext.BlockUIElementCollection;

            var table = new MarkdownTable(element.ColumnDefinitions!.Count, element.Rows!.Count, TableBorderThickness, TableBorderBrush!)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = TableMargin
            };

            // Add each row.
            for (var rowIndex = 0; rowIndex < element.Rows.Count; rowIndex++)
            {
                var row = element.Rows[rowIndex];

                // Add each cell.
                for (var cellIndex = 0; cellIndex < Math.Min(element.ColumnDefinitions.Count, row.Cells!.Count); cellIndex++)
                {
                    var cell = row.Cells[cellIndex];

                    // Cell content.
                    var cellContent = CreateOrReuseRichTextBlock(new UIElementCollectionRenderContext(null, context));
                    cellContent.Margin = TableCellPadding;
                    Grid.SetRow(cellContent, rowIndex);
                    Grid.SetColumn(cellContent, cellIndex);
                    switch (element.ColumnDefinitions[cellIndex].Alignment)
                    {
                        case ColumnAlignment.Center:
                            cellContent.TextAlignment = TextAlignment.Center;
                            break;

                        case ColumnAlignment.Right:
                            cellContent.TextAlignment = TextAlignment.Right;
                            break;
                    }

                    if (rowIndex == 0)
                    {
                        cellContent.FontWeight = FontWeights.Bold;
                    }

                    var paragraph = new Paragraph();

                    var childContext = new InlineRenderContext(paragraph.Inlines, context)
                    {
                        Parent = paragraph,
                        TrimLeadingWhitespace = true
                    };

                    RenderInlineChildren(cell.Inlines!, childContext);

                    cellContent.Blocks.Add(paragraph);
                    table.Children.Add(cellContent);
                }
            }

            blockUIElementCollection?.Add(table);
        }
    }
}