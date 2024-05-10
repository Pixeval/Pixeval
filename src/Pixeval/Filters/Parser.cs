using System;
using System.Collections.Generic;
using static Pixeval.Filters.IQueryNode;

namespace Pixeval.Filters;

public class Parser
{
    private IReadOnlyList<IQueryNode> Tokens { get; }

    private readonly TokenTreeNode _queryTokenTree;

    private TokenTreeNode _currentTopLevel;

    private Parser(IReadOnlyList<IQueryNode> filterTokens)
    {
        Tokens = filterTokens;
        _currentTopLevel = _queryTokenTree = new TokenTreeNode(TreeType.And, [], false);
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
    private TokenTreeNode Parse()
    {
        ParseArgumentList();
        if (Peek is not null)
        {
            throw new Exception("Unbalanced token");
        }
        return _queryTokenTree;
    }

    public static TokenTreeNode Parse(IReadOnlyList<IQueryNode> filterTokens) => new Parser(filterTokens).Parse();

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
            {
                return;
            }

            // If this position is overflow, throw error
            if (Position > Tokens.Count)
            {
                throw new Exception("Expected an argument, actual: empty token flow");
            }

            // If the correct token is meet, call to parse argument
            // Unexpected type of token, error
            if (!ParseArgument())
            {
                break;
            }
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
                        var nextAndNode = new TokenTreeNode(TreeType.And, [], isNot);
                        _currentTopLevel.Insert(nextAndNode);
                        _currentTopLevel = nextAndNode;

                        ParseAndList();

                        if (_currentTopLevel.Parent != null)
                        {
                            _currentTopLevel = _currentTopLevel.Parent;
                        }

                        break;
                    }
                    case Or:
                    {
                        var nextOrNode = new TokenTreeNode(TreeType.Or, [], isNot);
                        _currentTopLevel.Insert(nextOrNode);
                        _currentTopLevel = nextOrNode;

                        ParseOrList();

                        if (_currentTopLevel.Parent != null)
                        {
                            _currentTopLevel = _currentTopLevel.Parent;
                        }

                        break;
                    }
                    default:
                        throw new Exception("Expected either and/or token after eating left paren");
                }

                return true;

                void ParseAndList()
                {
                    _ = Eat<And>();
                    ParseArgumentList();
                    _ = Eat<RightParen>();
                }

                void ParseOrList()
                {
                    _ = Eat<Or>();
                    ParseArgumentList();
                    _ = Eat<RightParen>();
                }
            }
            // Data or keyword
            case Data:
            {
                var data = Eat<Data>();

                var tagToken = new StringToken(StringType.Title, data, isNot);
                _currentTopLevel.Insert(tagToken);

                return true;
            }
            case Hashtag:
            {
                _ = Eat<Hashtag>();
                var data = Eat<Data>();

                var tagToken = new StringToken(StringType.Tag, data, isNot);
                _currentTopLevel.Insert(tagToken);

                return true;
            }
            case Arobase:
            {
                _ = Eat<Arobase>();
                var author = Eat<Data>();

                var authorTagToken = new StringToken(StringType.Author, author, isNot);
                _currentTopLevel.Insert(authorTagToken);

                return true;
            }
            case A:
            {
                _ = Eat<A>();
                _ = Eat<Colon>();
                var author = Eat<Data>();

                var authorTagToken = new StringToken(StringType.Author, author, isNot);
                _currentTopLevel.Insert(authorTagToken);

                return true;
            }
            /*
            case IQueryFragmentNode.C:
            {
                Eat<C>();
                Eat<Colon>();
                var character = Eat<Data>();

                var characterTagToken = new TagToken(character);
                _currentTopLevel.Insert(characterTagToken);

                return true;
            }
            */
            case L:
            {
                _ = Eat<L>();
                _ = Eat<Colon>();
                var (start, end) = ParseRangeDesc();

                var rangeToken = new NumericRangeToken(RangeType.Bookmark, start, end, isNot);
                _currentTopLevel.Insert(rangeToken);

                return true;
            }
            case N:
            {
                _ = Eat<N>();
                _ = Eat<Colon>();
                var (start, end) = ParseRangeDesc();

                var rangeToken = new NumericRangeToken(RangeType.Index, start, end, isNot);
                _currentTopLevel.Insert(rangeToken);

                return true;
            }
            case S:
            {
                _ = Eat<S>();
                _ = Eat<Colon>();
                var (year, month, day) = ParseDateDesc();

                var date = new DateTime(year ?? DateTime.Today.Year, month, day);

                var dateOffset = new DateTimeOffset(date);

                var dateToken = new DateToken(RangeEdge.Starting, dateOffset, isNot);
                _currentTopLevel.Insert(dateToken);

                return true;
            }
            case E:
            {
                _ = Eat<E>();
                _ = Eat<Colon>();
                var (year, month, day) = ParseDateDesc();

                var date = new DateTime(year ?? DateTime.Today.Year, month, day);

                var dateOffset = new DateTimeOffset(date);

                var dateToken = new DateToken(RangeEdge.Ending, dateOffset, isNot);
                _currentTopLevel.Insert(dateToken);

                return true;
            }
        }

        return false;
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

