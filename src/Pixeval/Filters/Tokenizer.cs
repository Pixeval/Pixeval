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
    public static IList<IQueryToken> Tokenize(string src) => src switch
    {
        [] => [],
        ['+', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Plus()),
        ['-', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Dash()),
        ['.', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Dot()),
        ['@', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.At()),
        [':', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Colon()),
        [',', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Comma()),
        ['(', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.LeftParen()),
        ['[', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.LeftBracket()),
        [')', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.RightParen()),
        [']', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.RightBracket()),
        ['#', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Hashtag()),
        ['!', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Not()),
        ['l', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Colon()).Prepend(new IQueryToken.Like()),
        ['i', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Colon()).Prepend(new IQueryToken.Index()),
        ['s', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Colon()).Prepend(new IQueryToken.StartDate()),
        ['e', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Colon()).Prepend(new IQueryToken.EndDate()),
        ['a', 'n', 'd', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.And()),
        ['o', 'r', .. var rem] => Tokenize(rem).Prepend(new IQueryToken.Or()),
        ['"', .. var rem] => TokenizeAndPrepend(rem, t => t.IndexOf('"'), 1),
        [var x, ..] when char.IsWhiteSpace(x) => TokenizeAndSkip(src, char.IsWhiteSpace),
        [var x, ..] when char.IsDigit(x) => TokenizeNumeric(src),
        _ => TokenizeAndPrepend(src, EatDataPredicate)
    };

    private static bool EatDataPredicate(char ch)
    {
        return ch is ']' or ')' || char.IsWhiteSpace(ch);
    }

    public static IList<IQueryToken> TokenizeNumeric(string src)
    {
        var isData = false;
        for (var i = 0; i < src.Length; ++i)
        {
            var c = src[i];
            if (c is '-' or '.' or ',' or ']' or ')' || char.IsWhiteSpace(c))
                return Tokenize(src[i..]).Prepend(isData
                    ? new IQueryToken.Data(src[..i])
                    : new IQueryToken.Numeric(long.Parse(src[..i])));
            if (!char.IsDigit(c))
                isData = true;
        }

        return Tokenize("").Prepend(isData
            ? new IQueryToken.Data(src)
            : new IQueryToken.Numeric(long.Parse(src)));
    }

    private static IList<IQueryToken> TokenizeAndPrepend(string src, Func<string, int> indexOf, int offset = 0)
    {
        var index = indexOf(src);
        return Tokenize(src[(index + offset)..])
            .Prepend(new IQueryToken.Data(src[..index]));
    }

    private static IList<IQueryToken> TokenizeAndPrepend(string src, Func<char, bool> predicate, int offset = 0)
    {
        for (var i = 0; i < src.Length; ++i)
        {
            if (predicate(src[i]))
                return Tokenize(src[(i + offset)..])
                    .Prepend(new IQueryToken.Data(src[..i]));
        }

        return Tokenize("").Prepend(new IQueryToken.Data(src));
    }

    private static IList<IQueryToken> TokenizeAndSkip(string src, Func<char, bool> predicate, int offset = 0)
    {
        for (var i = 0; i < src.Length; ++i)
        {
            if (!predicate(src[i]))
                return Tokenize(src[(i + offset)..]);
        }
        return Tokenize("");
    }

    private static IList<IQueryToken> Prepend(this IList<IQueryToken> source,
        IQueryToken element)
    {
        source.Insert(0, element);
        return source;
    }

    private static IList<IQueryToken> Prepend(this IList<IQueryToken> source,
        IQueryToken.INullableToken element)
    {
        if (element.IsNotEmpty())
            source.Insert(0, element);
        return source;
    }
}

