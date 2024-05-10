using System;
using System.Collections.Generic;

namespace Pixeval.Filters;

public static class Tokenizer
{
    /// <summary>
    /// Tokenize function for tag parser, this function need to be reworked for performance in the future.
    /// Returns a flow of tokenized nodes carrying data of the input string, structured.
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static IList<IQueryNode> Tokenize(string src) => src switch
    {
    [] => [],
    ['+', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Plus()),
    ['-', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Dash()),
    ['.', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Dot()),
    ['@', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Arobase()),
    [':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()),
    [',', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Comma()),
    ['(', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.LeftParen()),
    ['[', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.LeftBracket()),
    [')', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.RightParen()),
    [']', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.RightBracket()),
    ['#', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Hashtag()),
    ['!', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Not()),
    ['e', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.EndDate()),
    ['l', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.Like()),
    ['i', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.Index()),
    ['s', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.StartDate()),
    ['a', 'n', 'd', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.And()),
    ['o', 'r', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Or()),
    ['"', .. var rem] => TokenizeAndPrepend(rem, t => t.IndexOf('"'), 1),
    [var x, ..] when char.IsWhiteSpace(x) => TokenizeAndSkip(src, char.IsWhiteSpace),
    [var x, ..] when char.IsDigit(x) => TokenizeNumeric(src),
        _ => TokenizeAndPrepend(src, EatDataPredicate)
    };

    private static bool EatDataPredicate(char ch)
    {
        return ch is ']' or ')' || char.IsWhiteSpace(ch);
    }

    public static IList<IQueryNode> TokenizeNumeric(string src)
    {
        var isData = false;
        for (var i = 0; i < src.Length; ++i)
        {
            var c = src[i];
            if (c is '-' or '.' or ',' or ']' or ')' || char.IsWhiteSpace(c))
                return Tokenize(src[i..]).Prepend(isData
                    ? new IQueryNode.Data(src[..i])
                    : new IQueryNode.Numeric(long.Parse(src[..i])));
            if (!char.IsDigit(c))
                isData = true;
        }

        return Tokenize("").Prepend(isData
            ? new IQueryNode.Data(src)
            : new IQueryNode.Numeric(long.Parse(src)));
    }

    private static IList<IQueryNode> TokenizeAndPrepend(string src, Func<string, int> indexOf, int offset = 0)
    {
        var index = indexOf(src);
        return Tokenize(src[(index + offset)..])
            .Prepend(new IQueryNode.Data(src[..index]));
    }

    private static IList<IQueryNode> TokenizeAndPrepend(string src, Func<char, bool> predicate, int offset = 0)
    {
        for (var i = 0; i < src.Length; ++i)
        {
            if (predicate(src[i]))
                return Tokenize(src[(i + offset)..])
                    .Prepend(new IQueryNode.Data(src[..i]));
        }

        return Tokenize("").Prepend(new IQueryNode.Data(src));
    }

    private static IList<IQueryNode> TokenizeAndSkip(string src, Func<char, bool> predicate, int offset = 0)
    {
        for (var i = 0; i < src.Length; ++i)
        {
            if (!predicate(src[i]))
                return Tokenize(src[(i + offset)..]);
        }
        return Tokenize("");
    }

    private static IList<IQueryNode> Prepend(this IList<IQueryNode> source,
        IQueryNode element)
    {
        source.Insert(0, element);
        return source;
    }

    private static IList<IQueryNode> Prepend(this IList<IQueryNode> source,
        IQueryNode.INullableNode element)
    {
        if (element.IsNotEmpty())
            source.Insert(0, element);
        return source;
    }
}

