using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Filters.TagParser;

internal class Tokenizer
{
    /// <summary>
    /// Put reserved symbols here, these symbols will be used for matching first character or used as skip condition while matching variable length tokens.
    /// </summary>
    internal string Reserved = "-#@:,([)]\"";

    /// <summary>
    /// Tokenize function for tag parser, this function need to be reworked for performance in the future.
    /// Returns a flow of tokenized nodes carrying data of the input string, structured.
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public IEnumerable<IQueryFragmentNode> Tokenize(string src)
        => src switch
        {
        [] => [],
        ['-', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.Dash()),
        ['#', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.Hashtag()),
        ['@', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.Arobase()),
        [':', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.Colon()),
        [',', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.Comma()),
        ['(', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.LeftParen()),
        ['[', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.LeftBracket()),
        [')', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.RightParen()),
        [']', .. var rem] => Tokenize(rem).Prepend(new IQueryFragmentNode.RightBracket()),
        ['a', ':', .. var rem] =>
            Tokenize(rem).Prepend(new IQueryFragmentNode.Colon()).Prepend(new IQueryFragmentNode.A()),
        ['c', ':', .. var rem] =>
            Tokenize(rem).Prepend(new IQueryFragmentNode.Colon()).Prepend(new IQueryFragmentNode.C()),
        ['"', .. var rem] =>
            Tokenize(string.Concat(rem.SkipWhile(ch => ch != '"').Skip(1)))
                .Prepend(new IQueryFragmentNode.Data(string.Concat(rem.TakeWhile(ch => ch != '"')))),
        [var x, .. var rem] when char.IsDigit(x) =>
            Tokenize(string.Concat(rem.SkipWhile(char.IsDigit)))
                .Prepend(new IQueryFragmentNode.Numeric(long.Parse(string.Concat(src.TakeWhile(char.IsDigit))))),
        [var x, .. var rem] when char.IsWhiteSpace(x) =>
            Tokenize(rem),
        [_, .. var rem] =>
            Tokenize(string.Concat(rem.SkipWhile(ch =>
                    !(char.IsWhiteSpace(ch) || Reserved.Contains(ch) || char.IsDigit(ch)))))
                .Prepend(new IQueryFragmentNode.Data(string.Concat(src.TakeWhile(ch =>
                    !(char.IsWhiteSpace(ch) || Reserved.Contains(ch) || char.IsDigit(ch))))))
        };
}
