// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
        var tok = Peek ?? ThrowUtils.MacroParse<T>(MacroParserResources.FilterTokenFinishedFormatted.Format(T.Name));
        if (tok is T t)
        {
            ++Position;
            return t;
        }

        return ThrowUtils.MacroParse<T>(MacroParserResources.UnexpactedTokenFormatted.Format(T.Name, tok));
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

    public static LeafSequence Parse(string str, out NumericRangeLeaf? index) => Parse((IReadOnlyList<IQueryToken>) Tokenizer.Tokenize(str), out index);

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
     *      ratio              ::= R COLON decimal_range_desc
     *
     *      range_desc         ::= interval_form | dash_form
     *          interval_form  ::= (LB | LP) NUM COMMA NUM (RB | RP)
     *          dash_form      ::= (DASH NUM) | (NUM DASH NUM) | (NUM DASH)
     *      date_desc          ::= [Num (DASH | DOT)] Num (DASH | DOT) Num
     *
     *      decimal_range_desc ::= (DASH DECIMAL) | (DECIMAL DASH DECIMAL) | (DECIMAL DASH)
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
                EatBool(false);
                return true;
            }
            case IQueryToken.Dash:
            {
                _ = Eat<IQueryToken.Dash>();
                EatBool(true);
                return true;
            }
            case IQueryToken.Like:
            {
                _ = Eat<IQueryToken.Like>();
                _ = EatRange(NumericRangeType.Bookmark);
                return true;
            }
            case IQueryToken.Index:
            {
                if (_index is not null)
                    ThrowUtils.MacroParse(MacroParserResources.IndexRangeUsedMoreThanOnce);
                _ = Eat<IQueryToken.Index>();
                _index = EatRange(NumericRangeType.Index);
                return true;
            }
            case IQueryToken.StartDate:
            {
                _ = Eat<IQueryToken.StartDate>();
                EatDate(DateRangeEdge.Starting);
                return true;
            }
            case IQueryToken.EndDate:
            {
                _ = Eat<IQueryToken.EndDate>();
                EatDate(DateRangeEdge.Ending);
                return true;
            }
            case IQueryToken.Ratio:
            {
                _ = Eat<IQueryToken.Ratio>();
                _ = EatFloatRange(FloatRangeType.Ratio);
                return true;
            }
        }

        return false;

        void EatSequence(SequenceType type)
        {
            var nextAndNode = new LeafSequence(type, [], isNot);
            _currentTopLevel.Insert(nextAndNode);
            _currentTopLevel = nextAndNode;

            _ = type is SequenceType.And ? Eat<IQueryToken.And>() : (IQueryToken) Eat<IQueryToken.Or>();
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

        NumericRangeLeaf EatRange(NumericRangeType type)
        {
            _ = Eat<IQueryToken.Colon>();
            var range = ParseRangeDesc();
            var rangeToken = new NumericRangeLeaf(type, range, isNot);
            _currentTopLevel.Insert(rangeToken);
            return rangeToken;
        }

        FloatRangeLeaf EatFloatRange(FloatRangeType type)
        {
            _ = Eat<IQueryToken.Colon>();
            var range = ParseFloatRangeDesc();
            var rangeToken = new FloatRangeLeaf(type, range, isNot);
            _currentTopLevel.Insert(rangeToken);
            return rangeToken;
        }

        void EatDate(DateRangeEdge type)
        {
            _ = Eat<IQueryToken.Colon>();
            var (year, month, day) = ParseDateDesc();

            var date = new DateTime(year ?? DateTime.Today.Year, month, day);

            var dateOffset = new DateTimeOffset(date);

            var dateToken = new DateLeaf(type, dateOffset, isNot);
            _currentTopLevel.Insert(dateToken);
        }

        void EatBool(bool isExclude)
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

            var rangeToken = new BoolLeaf(isExclude, type, isNot);
            _currentTopLevel.Insert(rangeToken);
        }
    }

    private Range<long> ParseRangeDesc()
    {
        var range = Peek switch
        {
            IQueryToken.LeftBracket or IQueryToken.LeftParen => ParseIntervalForm(),
            IQueryToken.Dash or IQueryToken.Numeric => ParseDashForm(),
            _ => (0, null)
        };

        return new Range<long>(range.Item1, range.Item2 ?? 0, range.Item2 is null);

        (long, long) ParseIntervalForm()
        {
            var leftInclusive = false;

            switch (Peek)
            {
                case IQueryToken.LeftBracket:
                    _ = Eat<IQueryToken.LeftBracket>();
                    leftInclusive = true;
                    break;
                case IQueryToken.LeftParen:
                    _ = Eat<IQueryToken.LeftParen>();
                    break;
            }

            var oa = ParseIntegerNumber();
            var a = oa - 1;
            _ = Eat<IQueryToken.Comma>();
            var ob = ParseIntegerNumber();
            var b = ob - 1;

            var rightInclusive = false;
            switch (Peek)
            {
                case IQueryToken.RightBracket:
                    _ = Eat<IQueryToken.RightBracket>();
                    rightInclusive = true;
                    break;
                case IQueryToken.RightParen:
                    _ = Eat<IQueryToken.RightParen>();
                    break;
                default:
                    return ThrowUtils.MacroParse<(long, long)>(MacroParserResources.ExpectedRightBracketOrParenInRangeFormatted.Format(Peek));
            }

            if (!leftInclusive)
                a += 1;
            if (rightInclusive)
                b -= 1;
            if (a < 0)
                ThrowUtils.MacroParse(MacroParserResources.NumericTooSmallInRangeFormatted.Format(oa));
            if (a > b)
                ThrowUtils.MacroParse(MacroParserResources.MinimumShouldBeSmallerThanMaximiumFormatted.Format(oa, ob));

            return (a, b);
        }

        (long, long?) ParseDashForm()
        {
            switch (Peek)
            {
                case IQueryToken.Dash:
                {
                    _ = Eat<IQueryToken.Dash>();
                    var b = ParseIntegerNumber();
                    return (0, b);
                }
                case IQueryToken.Numeric:
                {
                    // 为了符合从1开始的习惯，这里减一，即：
                    // 1-：第一张及以后
                    // 1-2：第一张及第二张图
                    var oa = ParseIntegerNumber();
                    var a = oa - 1;
                    if (a < 0)
                        ThrowUtils.MacroParse(MacroParserResources.NumericTooSmallInRangeFormatted.Format(oa));
                    _ = Eat<IQueryToken.Dash>();
                    if (Peek is IQueryToken.Numeric)
                    {
                        var b = ParseIntegerNumber();
                        if (a > b)
                            ThrowUtils.MacroParse(MacroParserResources.MinimumShouldBeSmallerThanMaximiumFormatted.Format(oa, b));
                        return (a, b);
                    }

                    return (a, null);
                }
                default:
                    return (0, null);
            }
        }
    }

    private Range<double> ParseFloatRangeDesc()
    {
        (double, double?) range;
        switch (Peek)
        {
            case IQueryToken.Dash:
            {
                _ = Eat<IQueryToken.Dash>();
                var b = ParseDecimalNumber();
                range = (0, b);
                break;
            }
            case IQueryToken.Numeric:
            {
                var a = ParseDecimalNumber();
                _ = Eat<IQueryToken.Dash>();
                if (Peek is IQueryToken.Numeric)
                {
                    var b = ParseDecimalNumber();
                    if (a > b)
                        ThrowUtils.MacroParse(MacroParserResources.MinimumShouldBeSmallerThanMaximiumFormatted.Format(a, b));
                    range = (a, b);
                    break;
                }

                range = (a, null);
                break;
            }
            default:
                range = (0, null);
                break;
        }

        return new Range<double>(range.Item1, range.Item2 ?? 0, range.Item2 is null);
    }

    private long ParseIntegerNumber()
        => Eat<IQueryToken.Numeric>().Value;

    /// <summary>
    /// 读取一个小数，可以是分号形式，例如1/3，或者小数点形式，例如1.23。
    /// <br/>
    /// 如果既没有小数点也没有分号，会返回一个小数形式的整数。
    /// <br/>
    /// 否则会读取3个token并且拼接字符串来还原小数。
    /// </summary>
    /// <returns>返回一个小数</returns>
    private double ParseDecimalNumber()
    {
        var num = Eat<IQueryToken.Numeric>();
        switch (Peek)
        {
            case IQueryToken.Slash:
            {
                _ = Eat<IQueryToken.Slash>();
                var otherNum = Eat<IQueryToken.Numeric>().Value;
                return (double) num.Value / otherNum;
            }
            case IQueryToken.Dot:
                _ = Eat<IQueryToken.Dot>();
                var frac = Eat<IQueryToken.Numeric>();
                return double.Parse($"{num}.{frac}");
            default:
                return num.Value;
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

    public static int TryNarrow(long value)
    {
        if (value > int.MaxValue)
            ThrowUtils.MacroParse(MacroParserResources.NumericTooLargeFormatted.Format(value));
        return (int) value;
    }
}
