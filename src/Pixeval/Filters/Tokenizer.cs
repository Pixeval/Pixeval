using System;
using System.Collections.Generic;

namespace Pixeval.Filters;

public static class Tokenizer
{
    /// <summary>
    /// Put reserved symbols here, these symbols will be used for matching first character or used as skip condition while matching variable length tokens.
    /// </summary>
    internal const string Reserved = "-.#@:,[]()\"!";

    internal const string DisallowedInTags = "#@:,[]()\"!";

    /// <summary>
    /// Tokenize function for tag parser, this function need to be reworked for performance in the future.
    /// Returns a flow of tokenized nodes carrying data of the input string, structured.
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static IList<IQueryNode> Tokenize(string src) => src switch
    {
        [] => [],
        ['-', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Dash()),
        ['.', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Dot()),
        ['@', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Arobase()),
        [':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()),
        [',', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Comma()),
        ['(', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.LeftParen()),
        ['[', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.LeftBracket()),
        [')', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.RightParen()),
        [']', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.RightBracket()),
        ['a', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.A()),
        ['c', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.C()),
        ['e', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.E()),
        ['l', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.L()),
        ['n', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.N()),
        ['s', ':', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Colon()).Prepend(new IQueryNode.S()),
        ['a', 'n', 'd', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.And()),
        ['o', 'r', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Or()),
        ['!', .. var rem] => Tokenize(rem).Prepend(new IQueryNode.Not()),
        ['#', .. var rem] => TokenizeAndPrepend(rem, EatDataPredicate).Prepend(new IQueryNode.Hashtag()),
        ['"', .. var rem] => TokenizeAndPrepend(rem, t => t.IndexOf('"'), 1),
        [var x, ..] when char.IsWhiteSpace(x) => TokenizeAndSkip(src, char.IsWhiteSpace),
        [var x, ..] when char.IsDigit(x) => TokenizeNumeric(src),
        _ => TokenizeAndPrepend(src, EatDataPredicate)
    };

    public static IList<IQueryNode> TokenizeNumeric(string src)
    {
        var isData = false;
        for (var i = 0; i < src.Length; ++i)
        {
            var c = src[i];
            if (EatDataPredicate(c))
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

    private static bool EatDataPredicate(char ch)
    {
        return Reserved.Contains(ch) || char.IsWhiteSpace(ch);
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

