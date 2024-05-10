using System;
using System.Collections.Generic;
using static Pixeval.Filters.IQueryNode;

namespace Pixeval.Filters;

public class Parser
{
    private IReadOnlyList<IQueryNode> Tokens { get; }

    private readonly TokenSequence _queryTokenTree;

    private TokenSequence _currentTopLevel;

    private Parser(IReadOnlyList<IQueryNode> filterTokens)
    {
        Tokens = filterTokens;
        _currentTopLevel = _queryTokenTree = new TokenSequence(SequenceType.And, [], false);
    }

    private int Position { get; set; }

    private T Eat<T>() where T : IQueryNode
    {
        var tok = Peek ?? throw new Exception("FilterToken finished");
        if (tok is T t)
        {
            Position++;
            return t;
        }

        throw new Exception($"Expected {typeof(T).Name}, actual: {tok?.GetType().Name}");
    }

    private IQueryNode? Peek => Position >= Tokens.Count ? null : Tokens[Position];

    /// <summary>
    /// top-level entrypoint
    /// </summary>
    private TokenSequence Parse()
    {
        ParseArgumentList();
        return Peek is null ? _queryTokenTree : throw new Exception("Unbalanced token");
    }

    public static TokenSequence Parse(IReadOnlyList<IQueryNode> filterTokens) => new Parser(filterTokens).Parse();

    /*
     * 
     *      filter          ::= argument_list
     *      argument_list   ::= {} | argument argument_list
     *      argument        ::=
     *                         and_list
     *                       | or_list
     *                       | keyword
     *                       | tag
     *                       | author
     *                       | char
     *                       | like_range
     *                       | index_range
     *                       | starting_date
     *                       | ending_date
     *
     *      and_list    ::= LP AND argument_list RP
     *      or_list     ::= LP OR argument_list RP
     *
     *      keyword     ::= DATA | STRING
     *      tag         ::= HASH DATA
     *      author      ::= AROBASE DATA | A COLON DATA
     *      char        ::= C COLON DATA
     *      like_range  ::= L COLON range_desc
     *      sequence_range ::= N COLON range_desc
     *      starting_date  ::= S COLON date_desc
     *      ending_date    ::= E COLON date_desc
     *
     *      range_desc     ::= interval_form | dash_form
     *      interval_form  ::= (LB | LP) NUM COMMA NUM (RB | RP)
     *      dash_form      ::= DASH NUM | NUM DASH NUM?
     *
     *      date_desc      ::= 
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
        if (Peek is Not)
        {
            _ = Eat<Not>();
            isNot = true;
        }

        switch (Peek)
        {
            // And/Or
            case LeftParen:
            {
                _ = Eat<LeftParen>();
                switch (Peek)
                {
                    case And:
                    {
                        EatSequence(SequenceType.And);
                        break;
                    }
                    case Or:
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
            case Data:
            {
                EatString(StringType.Title);
                return true;
            }
            case Hashtag:
            {
                _ = Eat<Hashtag>();
                EatString(StringType.Tag);
                return true;
            }
            case Arobase:
            {
                _ = Eat<Arobase>();
                EatString(StringType.Author);
                return true;
            }
            case Plus:
            {
                _ = Eat<Plus>();
                EatBool(true);
                return true;
            }
            case Dash:
            {
                _ = Eat<Dash>();
                EatBool(false);
                return true;
            }
            case Like:
            {
                _ = Eat<Like>();
                EatRange(RangeType.Bookmark);
                return true;
            }
            case IQueryNode.Index:
            {
                _ = Eat<IQueryNode.Index>();
                EatRange(RangeType.Index);
                return true;
            }
            case StartDate:
            {
                _ = Eat<StartDate>();
                EatDate(RangeEdge.Starting);
                return true;
            }
            case EndDate:
            {
                _ = Eat<EndDate>();
                EatDate(RangeEdge.Ending);
                return true;
            }
        }

        return false;

        void EatSequence(SequenceType type)
        {
            var nextAndNode = new TokenSequence(type, [], isNot);
            _currentTopLevel.Insert(nextAndNode);
            _currentTopLevel = nextAndNode;

            _ = type is SequenceType.And ? Eat<And>() : (IQueryNode)Eat<Or>();
            ParseArgumentList();
            _ = Eat<RightParen>();

            if (_currentTopLevel.Parent != null)
            {
                _currentTopLevel = _currentTopLevel.Parent;
            }
        }

        void EatString(StringType type)
        {
            var data = Eat<Data>();
            var authorTagToken = new StringToken(type, data, isNot);
            _currentTopLevel.Insert(authorTagToken);
        }

        void EatRange(RangeType type)
        {
            _ = Eat<Colon>();
            var (start, end) = ParseRangeDesc();
            var rangeToken = new NumericRangeToken(type, start, end, isNot);
            _currentTopLevel.Insert(rangeToken);
        }

        void EatDate(RangeEdge type)
        {
            _ = Eat<Colon>();
            var (year, month, day) = ParseDateDesc();

            var date = new DateTime(year ?? DateTime.Today.Year, month, day);

            var dateOffset = new DateTimeOffset(date);

            var dateToken = new DateToken(type, dateOffset, isNot);
            _currentTopLevel.Insert(dateToken);
        }

        void EatBool(bool isInclude)
        {
            var content = Eat<Data>();
            var type = content.Value.ToLower() switch
            {
                "r18" => BoolType.R18,
                "r18g" => BoolType.R18G,
                "gif" => BoolType.Gif,
                _ => throw new Exception("Invalid bool type")
            };

            var rangeToken = new BoolToken(isInclude, type, isNot);
            _currentTopLevel.Insert(rangeToken);
        }
    }

    private (long?, long?) ParseRangeDesc()
    {
        return Peek switch
        {
            LeftBracket or LeftParen => ParseIntervalForm(),
            Dash or Numeric => ParseDashForm(),
            _ => (null, null)
        };

        (long, long) ParseIntervalForm()
        {
            var leftInclusive = false;

            switch (Peek)
            {
                case LeftBracket:
                    _ = Eat<LeftBracket>();
                    leftInclusive = true;
                    break;
                case LeftParen:
                    _ = Eat<LeftParen>();
                    leftInclusive = false;
                    break;
            }

            var a = Eat<Numeric>();
            _ = Eat<Comma>();
            var b = Eat<Numeric>();

            var rightInclusive = false;
            switch (Peek)
            {
                case RightBracket:
                    _ = Eat<RightBracket>();
                    rightInclusive = true;
                    break;
                case RightParen:
                    _ = Eat<RightParen>();
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
                case Dash:
                {
                    _ = Eat<Dash>();
                    var a = Eat<Numeric>();
                    return (null, a.Value);
                }
                case Numeric:
                {
                    var a = Eat<Numeric>();
                    _ = Eat<Dash>();
                    if (Peek is Numeric)
                    {
                        var b = Eat<Numeric>();
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
        var firstPart = Eat<Numeric>();
        if (TryEatDateSeparator())
        {
            var secondPart = Eat<Numeric>();

            if (TryEatDateSeparator())
            {
                var thirdPart = Eat<Numeric>();

                return ((int)firstPart.Value, (int)secondPart.Value, (int)thirdPart.Value);
            }

            return (null, (int)firstPart.Value, (int)secondPart.Value);
        }

        throw new Exception("At least two numeric parts should be contained in a date segment");

        bool TryEatDateSeparator()
        {
            switch (Peek)
            {
                case Dash:
                {
                    _ = Eat<Dash>();
                    return true;
                }
                case Dot:
                {
                    _ = Eat<Dot>();
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}

