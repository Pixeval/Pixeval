using System;
using System.Collections.Generic;
using Pixeval.Utilities;

namespace Pixeval.Filters;

public class Parser
{
    private IReadOnlyList<IQueryToken> Tokens { get; }

    private readonly LeafSequence _queryTokenTree;

    private LeafSequence _currentTopLevel;

    private NumericRangeLeaf? _index;

    private Parser(IReadOnlyList<IQueryToken> filterTokens)
    {
        Tokens = filterTokens;
        _currentTopLevel = _queryTokenTree = new LeafSequence(SequenceType.And, [], false);
    }

    private int Position { get; set; }

    private T Eat<T>() where T : IQueryToken
    {
        var tok = Peek ?? ThrowUtils.MacroParse<T>(MacroParserResources.FilterTokenFinishedFormatted.Format(typeof(T)));
        if (tok is T t)
        {
            Position++;
            return t;
        }

        return ThrowUtils.MacroParse<T>(MacroParserResources.UnexpactedTokenFormatted.Format(typeof(T), tok.GetType()));
    }

    private IQueryToken? Peek => Position >= Tokens.Count ? null : Tokens[Position];

    /// <summary>
    /// top-level entrypoint
    /// </summary>
    private LeafSequence Parse(out NumericRangeLeaf? index)
    {
        _index = null;
        ParseArgumentList();
        index = _index;
        return Peek is null ? _queryTokenTree : ThrowUtils.MacroParse<LeafSequence>(MacroParserResources.UnbalancedParFormatted.Format(Peek));
    }

    public static LeafSequence Parse(string str, out NumericRangeLeaf? index) => Parse((IReadOnlyList<IQueryToken>)Tokenizer.Tokenize(str), out index);

    public static LeafSequence Parse(IReadOnlyList<IQueryToken> filterTokens, out NumericRangeLeaf? index) => new Parser(filterTokens).Parse(out index);

    /*
     *
     *      filter             ::= argument_list
     *      argument_list      ::= {} | argument argument_list
     *      argument           ::= and_list   | or_list     | title
     *                           | tag        | author      | like_bool
     *                           | like_range | index_range | starting_date
     *                           | ending_date
     *
     *      and_list           ::= LP AND argument_list RP
     *      or_list            ::= LP OR argument_list RP
     *
     *      title              ::= DATA
     *      tag                ::= HASH DATA
     *      author             ::= AT DATA
     *      like_bool          ::= (ADD | DASH) enum
     *          enum           ::= R18 | R18G | GIF
     *      like_range         ::= L COLON range_desc
     *      sequence_range     ::= I COLON range_desc
     *      starting_date      ::= S COLON date_desc
     *      ending_date        ::= E COLON date_desc
     *
     *      range_desc         ::= interval_form | dash_form
     *          interval_form  ::= (LB | LP) NUM COMMA NUM (RB | RP)
     *          dash_form      ::= (DASH NUM) | (NUM DASH NUM) | (NUM DASH)
     *      date_desc          ::= [Num (DASH | DOT)] Num (DASH | DOT) Num
     */

    /*
     * Parser rules
     * 
     */

    private void ParseArgumentList()
    {
        while (true)
        {
            // If all tokens are eaten, return gracefully
            if (Position == Tokens.Count)
                return;

            // If this position is overflow, throw error
            if (Position > Tokens.Count)
                ThrowUtils.MacroParse(MacroParserResources.ParserOutOfRange);

            // If the correct token is meet, call to parse argument
            // Unexpected type of token, error
            if (!ParseArgument())
                return;
        }
    }

    private bool ParseArgument()
    {
        var isNot = false;
        if (Peek is IQueryToken.Not)
        {
            _ = Eat<IQueryToken.Not>();
            isNot = true;
        }

        switch (Peek)
        {
            // And/Or
            case IQueryToken.LeftParen:
            {
                _ = Eat<IQueryToken.LeftParen>();
                switch (Peek)
                {
                    case IQueryToken.And:
                    {
                        EatSequence(SequenceType.And);
                        break;
                    }
                    case IQueryToken.Or:
                    {
                        EatSequence(SequenceType.Or);
                        break;
                    }
                    default:
                        return ThrowUtils.MacroParse<bool>(MacroParserResources.ExpectedAndOrAfterLeftParFormatted.Format(Peek));
                }

                return true;
            }
            // Data or keyword
            case IQueryToken.Data:
            {
                EatString(StringType.Title);
                return true;
            }
            case IQueryToken.Hashtag:
            {
                _ = Eat<IQueryToken.Hashtag>();
                EatString(StringType.Tag);
                return true;
            }
            case IQueryToken.At:
            {
                switch (Peek)
                {
                    case IQueryToken.At:
                    {
                        _ = Eat<IQueryToken.At>();
                        EatString(StringType.Author);
                        break;
                    }
                    case IQueryToken.Numeric:
                    {
                        _ = Eat<IQueryToken.Numeric>();
                        EatNumeric();
                        break;
                    }
                    default:
                        return ThrowUtils.MacroParse<bool>(MacroParserResources.ExpectedTokenAfterAtMarkFormatted.Format(Peek));
                }
                return true;
            }
            case IQueryToken.Plus:
            {
                _ = Eat<IQueryToken.Plus>();
                EatBool(true);
                return true;
            }
            case IQueryToken.Dash:
            {
                _ = Eat<IQueryToken.Dash>();
                EatBool(false);
                return true;
            }
            case IQueryToken.Like:
            {
                _ = Eat<IQueryToken.Like>();
                _ = EatRange(RangeType.Bookmark);
                return true;
            }
            case IQueryToken.Index:
            {
                if (_index is not null)
                    ThrowUtils.MacroParse(MacroParserResources.IndexRangeUsedMoreThanOnce);
                _ = Eat<IQueryToken.Index>();
                _index = EatRange(RangeType.Index);
                return true;
            }
            case IQueryToken.StartDate:
            {
                _ = Eat<IQueryToken.StartDate>();
                EatDate(RangeEdge.Starting);
                return true;
            }
            case IQueryToken.EndDate:
            {
                _ = Eat<IQueryToken.EndDate>();
                EatDate(RangeEdge.Ending);
                return true;
            }
        }

        return false;

        void EatSequence(SequenceType type)
        {
            var nextAndNode = new LeafSequence(type, [], isNot);
            _currentTopLevel.Insert(nextAndNode);
            _currentTopLevel = nextAndNode;

            _ = type is SequenceType.And ? Eat<IQueryToken.And>() : (IQueryToken)Eat<IQueryToken.Or>();
            ParseArgumentList();
            _ = Eat<IQueryToken.RightParen>();

            if (_currentTopLevel.Parent != null)
            {
                _currentTopLevel = _currentTopLevel.Parent;
            }
        }

        void EatString(StringType type)
        {
            var data = Eat<IQueryToken.Data>();
            var authorTagToken = new StringLeaf(type, data, isNot);
            _currentTopLevel.Insert(authorTagToken);
        }

        void EatNumeric()
        {
            var numeric = Eat<IQueryToken.Numeric>();
            var authorTagToken = new NumericLeaf(numeric.Value, isNot);
            _currentTopLevel.Insert(authorTagToken);
        }

        NumericRangeLeaf EatRange(RangeType type)
        {
            _ = Eat<IQueryToken.Colon>();
            var range = ParseRangeDesc();
            var rangeToken = new NumericRangeLeaf(type, range, isNot);
            _currentTopLevel.Insert(rangeToken);
            return rangeToken;
        }

        void EatDate(RangeEdge type)
        {
            _ = Eat<IQueryToken.Colon>();
            var (year, month, day) = ParseDateDesc();

            var date = new DateTime(year ?? DateTime.Today.Year, month, day);

            var dateOffset = new DateTimeOffset(date);

            var dateToken = new DateLeaf(type, dateOffset, isNot);
            _currentTopLevel.Insert(dateToken);
        }

        void EatBool(bool isInclude)
        {
            var content = Eat<IQueryToken.Data>();
            var type = content.Value.ToLower() switch
            {
                "r18" => BoolType.R18,
                "r18g" => BoolType.R18G,
                "ai" => BoolType.Ai,
                "gif" => BoolType.Gif,
                _ => ThrowUtils.MacroParse<BoolType>(MacroParserResources.InvalidConstraintFormatted.Format("r18, r18g, ai, gif", content.Value))
            };

            var rangeToken = new BoolLeaf(isInclude, type, isNot);
            _currentTopLevel.Insert(rangeToken);
        }
    }

    private Range ParseRangeDesc()
    {
        var range = Peek switch
        {
            IQueryToken.LeftBracket or IQueryToken.LeftParen => ParseIntervalForm(),
            IQueryToken.Dash or IQueryToken.Numeric => ParseDashForm(),
            _ => (0, null)
        };

        return new Range(new Index(TryNarrow(range.Item1)), range.Item2 is { } l ? new Index(TryNarrow(l)) : new Index(0, true));

        (long, long) ParseIntervalForm()
        {
            var leftInclusive = false;

            if (Peek is IQueryToken.LeftBracket)
            {
                _ = Eat<IQueryToken.LeftBracket>();
                leftInclusive = true;
            }

            var a = Eat<IQueryToken.Numeric>();
            _ = Eat<IQueryToken.Comma>();
            var b = Eat<IQueryToken.Numeric>();

            var rightInclusive = false;
            if (Peek is IQueryToken.RightBracket)
            {
                _ = Eat<IQueryToken.RightBracket>();
                rightInclusive = true;
            }

            return (leftInclusive, rightInclusive) switch
            {
                (true, true) => (a.Value, b.Value - 1),
                (false, true) => (a.Value + 1, b.Value - 1),
                (true, false) => (a.Value, b.Value),
                (false, false) => (a.Value + 1, b.Value)
            };
        }

        (long, long?) ParseDashForm()
        {
            switch (Peek)
            {
                case IQueryToken.Dash:
                {
                    _ = Eat<IQueryToken.Dash>();
                    var a = Eat<IQueryToken.Numeric>();
                    return (0, a.Value);
                }
                case IQueryToken.Numeric:
                {
                    var a = Eat<IQueryToken.Numeric>();
                    _ = Eat<IQueryToken.Dash>();
                    if (Peek is IQueryToken.Numeric)
                    {
                        var b = Eat<IQueryToken.Numeric>();
                        return (a.Value, b.Value);
                    }

                    return (a.Value, null);
                }
                default:
                    return (0, null);
            }
        }
    }

    private (int?, int, int) ParseDateDesc()
    {
        var firstPart = TryNarrow(Eat<IQueryToken.Numeric>().Value);
        if (TryEatDateSeparator())
        {
            var secondPart = TryNarrow(Eat<IQueryToken.Numeric>().Value);

            if (TryEatDateSeparator())
            {
                var thirdPart = TryNarrow(Eat<IQueryToken.Numeric>().Value);

                return (firstPart, secondPart, thirdPart);
            }

            return (null, firstPart, secondPart);
        }

        return ThrowUtils.MacroParse<(int?, int, int)>(MacroParserResources.ExpectedAtLeastTwoNumericInDateFormatted.Format(firstPart));

        bool TryEatDateSeparator()
        {
            switch (Peek)
            {
                case IQueryToken.Dash:
                {
                    _ = Eat<IQueryToken.Dash>();
                    return true;
                }
                case IQueryToken.Dot:
                {
                    _ = Eat<IQueryToken.Dot>();
                    return true;
                }
                default:
                    return false;
            }
        }
    }

    private static int TryNarrow(long value)
    {
        if (value > int.MaxValue)
            ThrowUtils.MacroParse(MacroParserResources.NumericTooLargeFormatted.Format(value));
        return (int)value;
    }
}
