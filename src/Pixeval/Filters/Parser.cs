using System;
using System.Collections.Generic;

namespace Pixeval.Filters;

public class Parser
{
    private IReadOnlyList<IQueryToken> Tokens { get; }

    private readonly LeafSequence _queryTokenTree;

    private LeafSequence _currentTopLevel;

    private Parser(IReadOnlyList<IQueryToken> filterTokens)
    {
        Tokens = filterTokens;
        _currentTopLevel = _queryTokenTree = new LeafSequence(SequenceType.And, [], false);
    }

    private int Position { get; set; }

    private T Eat<T>() where T : IQueryToken
    {
        var tok = Peek ?? throw new Exception("FilterToken finished");
        if (tok is T t)
        {
            Position++;
            return t;
        }

        throw new Exception($"Expected {typeof(T).Name}, actual: {tok?.GetType().Name}");
    }

    private IQueryToken? Peek => Position >= Tokens.Count ? null : Tokens[Position];

    /// <summary>
    /// top-level entrypoint
    /// </summary>
    private LeafSequence Parse()
    {
        ParseArgumentList();
        return Peek is null ? _queryTokenTree : throw new Exception("Unbalanced token");
    }

    public static LeafSequence Parse(string str) => Parse((IReadOnlyList<IQueryToken>)Tokenizer.Tokenize(str));

    public static LeafSequence Parse(IReadOnlyList<IQueryToken> filterTokens) => new Parser(filterTokens).Parse();

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
                throw new Exception("Expected an argument, actual: empty token flow");

            // If the correct token is meet, call to parse argument
            // Unexpected type of token, error
            if (!ParseArgument())
                break;
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
                        throw new Exception("Expected either and/or token after eating left paren");
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
                _ = Eat<IQueryToken.At>();
                EatString(StringType.Author);
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
                EatRange(RangeType.Bookmark);
                return true;
            }
            case IQueryToken.Index:
            {
                _ = Eat<IQueryToken.Index>();
                EatRange(RangeType.Index);
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

        void EatRange(RangeType type)
        {
            _ = Eat<IQueryToken.Colon>();
            var (start, end) = ParseRangeDesc();
            var rangeToken = new NumericRangeLeaf(type, start, end, isNot);
            _currentTopLevel.Insert(rangeToken);
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
                "gif" => BoolType.Gif,
                _ => throw new Exception("Invalid bool type")
            };

            var rangeToken = new BoolLeaf(isInclude, type, isNot);
            _currentTopLevel.Insert(rangeToken);
        }
    }

    private (long?, long?) ParseRangeDesc()
    {
        return Peek switch
        {
            IQueryToken.LeftBracket or IQueryToken.LeftParen => ParseIntervalForm(),
            IQueryToken.Dash or IQueryToken.Numeric => ParseDashForm(),
            _ => (null, null)
        };

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
                    leftInclusive = false;
                    break;
            }

            var a = Eat<IQueryToken.Numeric>();
            _ = Eat<IQueryToken.Comma>();
            var b = Eat<IQueryToken.Numeric>();

            var rightInclusive = false;
            switch (Peek)
            {
                case IQueryToken.RightBracket:
                    _ = Eat<IQueryToken.RightBracket>();
                    rightInclusive = true;
                    break;
                case IQueryToken.RightParen:
                    _ = Eat<IQueryToken.RightParen>();
                    rightInclusive = false;
                    break;
            }

            return (leftInclusive, rightInclusive) switch
            {
                (true, true) => (a.Value, b.Value - 1),
                (false, true) => (a.Value + 1, b.Value - 1),
                (true, false) => (a.Value, b.Value),
                (false, false) => (a.Value + 1, b.Value)
            };
        }

        (long?, long?) ParseDashForm()
        {
            switch (Peek)
            {
                case IQueryToken.Dash:
                {
                    _ = Eat<IQueryToken.Dash>();
                    var a = Eat<IQueryToken.Numeric>();
                    return (null, a.Value);
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
                    return (null, null);
            }
        }
    }

    private (int?, int, int) ParseDateDesc()
    {
        var firstPart = Eat<IQueryToken.Numeric>();
        if (TryEatDateSeparator())
        {
            var secondPart = Eat<IQueryToken.Numeric>();

            if (TryEatDateSeparator())
            {
                var thirdPart = Eat<IQueryToken.Numeric>();

                return ((int)firstPart.Value, (int)secondPart.Value, (int)thirdPart.Value);
            }

            return (null, (int)firstPart.Value, (int)secondPart.Value);
        }

        throw new Exception("At least two numeric parts should be contained in a date segment");

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
}
