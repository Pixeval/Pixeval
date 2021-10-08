using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks
{
    /// <summary>
    /// Yaml Header. use for blog.
    /// e.g.
    /// ---
    /// title: something
    /// tag: something
    /// ---
    /// </summary>
    public class YamlHeaderBlock : MarkdownBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YamlHeaderBlock"/> class.
        /// </summary>
        public YamlHeaderBlock()
            : base(MarkdownBlockType.YamlHeader)
        {
        }

        /// <summary>
        /// Gets or sets yaml header properties
        /// </summary>
        public Dictionary<string, string>? Children { get; set; }

        /// <summary>
        /// Parse yaml header
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location of the first hash character. </param>
        /// <param name="end"> The location of the end of the line. </param>
        /// <param name="realEndIndex"> The location of the actual end of the parse. </param>
        /// <returns>Parsed <see cref="YamlHeaderBlock"/> class</returns>
        internal static YamlHeaderBlock? Parse(string markdown, int start, int end, out int realEndIndex)
        {
            // As yaml header, must be start a line with "---"
            // and end with a line "---"
            realEndIndex = start;
            if (end - start < 3)
            {
                return null;
            }

            if (start != 0 || markdown.Substring(start, 3) != "---")
            {
                return null;
            }

            var startUnderlineIndex = Common.FindNextSingleNewLine(markdown, start, end, out var startOfNextLine);
            // ReSharper disable once UselessBinaryOperation
            if (startUnderlineIndex - start != 3)
            {
                return null;
            }

            var lockedFinalUnderline = false;

            // if current line not contain the ": ", check it is end of parse, if not, exit
            // if next line is the end, exit
            var pos = startOfNextLine;
            var elements = new List<string>();
            while (pos < end)
            {
                var nextUnderLineIndex = Common.FindNextSingleNewLine(markdown, pos, end, out startOfNextLine);
                var haveSeparator = markdown.Substring(pos, nextUnderLineIndex - pos).Contains(": ");
                if (haveSeparator)
                {
                    elements.Add(markdown.Substring(pos, nextUnderLineIndex - pos));
                }
                else if (end - pos >= 3 && markdown.Substring(pos, 3) == "---")
                {
                    lockedFinalUnderline = true;
                    break;
                }
                else if (startOfNextLine == pos + 1)
                {
                    pos = startOfNextLine;
                    continue;
                }
                else
                {
                    return null;
                }

                pos = startOfNextLine;
            }

            // if not have the end, return
            if (!lockedFinalUnderline)
            {
                return null;
            }

            // parse yaml header properties
            if (elements.Count < 1)
            {
                return null;
            }

            var result = new YamlHeaderBlock
            {
                Children = new Dictionary<string, string>()
            };
            foreach (var splits in elements.Select(item => item.Split(new[] { ": " }, StringSplitOptions.None)))
            {
                if (splits.Length < 2)
                {
                    continue;
                }

                var key = splits[0];
                var value = splits[1];
                if (key.Trim().Length == 0)
                {
                    continue;
                }

                value = string.IsNullOrEmpty(value.Trim()) ? string.Empty : value;
                result.Children.Add(key, value);
            }

            if (result.Children == null)
            {
                return null;
            }

            realEndIndex = pos + 3;
            return result;
        }

        /// <summary>
        /// Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string? ToString()
        {
            if (Children == null)
            {
                return base.ToString();
            }

            var result = Children.Aggregate(string.Empty, (current, item) => current + (item.Key + ": " + item.Value + "\n"));

            result.TrimEnd('\n');
            return result;
        }
    }
}