using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Filters.TagParser
{
    internal class Tokenizer
    {
        /**
         * Put reserved symbols here, these symbols will be used for matching first character or used as skip condition while matching variable length tokens.
         */
        internal string reserved = "-#@:,([)]\"";


        /**
         * Tokenize function for tag parser, this function need to be reworked for performance in future.
         * Returns a flow of tokenized nodes carrying data of the input string, structured.
         */
        public IEnumerable<QueryFragmentNode> Tokenize(string src)
            => src switch
            {
            [] => new List<QueryFragmentNode>(),
            ['-', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.Dash()),
            ['#', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.Hashtag()),
            ['@', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.Arobase()),
            [':', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.Colon()),
            [',', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.Comma()),
            ['(', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.LeftParen()),
            ['[', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.LeftBracket()),
            [')', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.RightParen()),
            [']', .. var rem] => Tokenize(rem).Prepend(new QueryFragmentNode.RightBracket()),
            ['a', ':', .. var rem] =>
                Tokenize(rem).Prepend(new QueryFragmentNode.Colon()).Prepend(new QueryFragmentNode.A()),
            ['c', ':', .. var rem] =>
                Tokenize(rem).Prepend(new QueryFragmentNode.Colon()).Prepend(new QueryFragmentNode.C()),
            ['"', .. var rem] =>
                Tokenize(string.Concat(rem.SkipWhile(ch => ch != '"').Skip(1)))
                    .Prepend(new QueryFragmentNode.Data(string.Concat(rem.TakeWhile(ch => ch != '"')))),
            [var x, .. var rem] when char.IsDigit(x) =>
                Tokenize(string.Concat(rem.SkipWhile(char.IsDigit)))
                    .Prepend(new QueryFragmentNode.Numeric(long.Parse(string.Concat(src.TakeWhile(char.IsDigit))))),
            [var x, .. var rem] when char.IsWhiteSpace(x) =>
                Tokenize(rem),
            [var _, .. var rem] =>
                Tokenize(string.Concat(rem.SkipWhile(ch =>
                        !(char.IsWhiteSpace(ch) || reserved.Contains(ch) || char.IsDigit(ch)))))
                    .Prepend(new QueryFragmentNode.Data(string.Concat(src.TakeWhile(ch =>
                        !(char.IsWhiteSpace(ch) || reserved.Contains(ch) || char.IsDigit(ch))))))
            };
    }
}
