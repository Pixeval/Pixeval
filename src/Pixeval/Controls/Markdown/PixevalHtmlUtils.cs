// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pixeval.Controls;

internal static partial class PixevalHtmlUtils
{
    private static readonly HashSet<string> EmptyTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source"
    };

    [GeneratedRegex(@"<(?'close'/?)[\t ]*(?'tagname'[a-z][a-z0-9]*)(?'attributes'[ \t][^>]*|/)?>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "zh-CN")]
    private static partial Regex TagPattern { get; }

    [GeneratedRegex(@"\n{2}", RegexOptions.Compiled)]
    private static partial Regex EmptyLinePattern { get; }

    public static Regex CreateTagstartPattern(IEnumerable<string> tags)
    {
        var tagList = string.Join("|", tags);
        return new Regex(
            @$"<[\t ]*(?'tagname'{tagList})(?'attributes'[ \t][^>]*|/)?>",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public static int SearchTagRange(string text, Match tagStartPatternMatch)
    {
        var searchStart = tagStartPatternMatch.Index + tagStartPatternMatch.Length;

        if (tagStartPatternMatch.Value.EndsWith("/>", StringComparison.Ordinal))
            return searchStart;

        var end = SearchTagEnd(text, searchStart, tagStartPatternMatch.Groups["tagname"].Value);
        return end is -1 ? text.Length : end;
    }

    public static int SearchTagRangeContinuous(string text, Match tagStartPatternMatch)
    {
        var index = SearchTagRange(text, tagStartPatternMatch);

        while (true)
        {
            if (text.Length - 1 <= index)
                return index;

            var emptyLine = EmptyLinePattern.Match(text, index);
            if (!emptyLine.Success)
                return text.Length - 1;

            var tag = TagPattern.Match(text, index);
            if (tag.Success && tag.Index < emptyLine.Index)
                index = SearchTagRange(text, tag);
            else
                return emptyLine.Index;
        }
    }

    private static int SearchTagEnd(string text, int start, string startTagName)
    {
        var tags = new Stack<string>();
        tags.Push(startTagName);

        while (true)
        {
            var isEmptyTag = EmptyTags.Contains(tags.Peek());
            var match = TagPattern.Match(text, start);

            if (isEmptyTag && (!match.Success || match.Index != start))
            {
                if (tags.Count is 1)
                    return start;

                tags.Pop();
            }

            if (!match.Success)
                return -1;

            start = match.Index + match.Length;

            if (match.Value.EndsWith("/>", StringComparison.Ordinal))
                continue;

            var tagName = match.Groups["tagname"].Value.ToLowerInvariant();

            if (!string.IsNullOrEmpty(match.Groups["close"].Value))
            {
                while (tags.Count > 0)
                {
                    var peekTag = tags.Peek();
                    tags.Pop();

                    if (peekTag.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                        break;
                }

                if (tags.Count is 0)
                    return match.Index + match.Length;
            }
            else
            {
                if (EmptyTags.Contains(tags.Peek()))
                    tags.Pop();

                tags.Push(match.Groups["tagname"].Value);
            }
        }
    }
}
