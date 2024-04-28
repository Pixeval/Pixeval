using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Filters.TagParser;

internal class Parser(List<IQueryFragmentNode> filterTokens)
{
    private List<IQueryFragmentNode> Tokens { get; } = filterTokens;

    private int Position { get; set; } = 0;

    private IQueryFragmentNode Eat()
    {
        if (Position == Tokens.Count)
        {
            throw new Exception("FilterToken finished");
        }
        var tok = Tokens[Position];
        Position++;
        return tok;
    }

    private IQueryFragmentNode CurrentFilterToken => Tokens[Position];

    private FilterSettingBuilder _filterSettingBuilder = new();

    public FilterSetting Build()
    {
        ParseFilter();
        _filterSettingBuilder.IncludeTags = _filterSettingBuilder.IncludeTags.ToList();
        _filterSettingBuilder.ExcludeTags = _filterSettingBuilder.ExcludeTags.ToList();
        _filterSettingBuilder.ExcludeUserName = _filterSettingBuilder.ExcludeUserName.ToList();
        return _filterSettingBuilder.Build();
    }

    /*
     * 
     *      filter          ::= argument_list
     *      argument_list   ::= {} | argument argument_list
     *      argument        ::= keyword
     *                       | tag
     *                       | author
     *                       | char
     *                       | like_range
     *                       | index_range
     *
     *      keyword     ::= DATA
     *      tag         ::= HASH DATA
     *      author      ::= AROBASE DATA | A COLON DATA
     *      char        ::= C COLON DATA
     *      like_range  ::= d"like" COLON range_desc
     *      index_range ::= d"index" COLON range_desc
     *
     *      range_desc     ::= interval_form | dash_form
     *      interval_form  ::= (LB | LP) NUM COMMA NUM (RB | RP)
     *      dash_form      ::= DASH NUM | NUM DASH NUM?
     */

    /*
     * Parser rules
     * 
     */

    /// <summary>
    /// top-level entrypoint
    /// </summary>
    public void ParseFilter()
    {
        ParseArgumentList();
    }

    public void ParseArgumentList()
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
        if (CurrentFilterToken is IQueryFragmentNode.Data or IQueryFragmentNode.Hashtag or IQueryFragmentNode.Arobase or IQueryFragmentNode.A or IQueryFragmentNode.C)
        {
            ParseArgument();
        }
        // Unexpected type of token, error
        else
        {
            throw new Exception("Not a starting token of argument");
        }
        ParseArgument();
        ParseArgumentList();
    }

    public void ParseArgument()
    {
        switch (CurrentFilterToken)
        {
            case IQueryFragmentNode.Data:
            {
                var data = EatData();

                switch (data.Value)
                {
                    // The current parsing for `like` and `index` are retrieving them from Data token, it would be better to put them into dedicated tokens
                    // in the current use case.
                    case "like":
                    {
                        EatColon();
                        var (lo, hi) = ParseRangeDesc();

                        if (lo is { } l)
                        {
                            _filterSettingBuilder.LeastBookmark = l;
                        }
                        if (hi is { } h)
                        {
                            _filterSettingBuilder.MaximumBookmark = h;
                        }

                        break;
                    }
                    case "index":
                    {
                        EatColon();
                        var (lo, hi) = ParseRangeDesc();
                

                        // local variable minIndex and maxIndex correspond to retrieved data, they are used no where in current FilterSetting definition
                        if (lo is { } l)
                        {
                            var minIndex = l;
                        }
                        if (hi is { } h)
                        {
                            var maxIndex = h;
                        }

                        break;
                    }
                    default:
                        _filterSettingBuilder.IncludeTags
                            = _filterSettingBuilder.IncludeTags.Append(new QueryFilterToken(data.Value));
                        break;
                }
                return;
            }
            case IQueryFragmentNode.Hashtag:
            {
                EatHash();
                var tag = EatData();
                _filterSettingBuilder.IncludeTags
                    = _filterSettingBuilder.IncludeTags.Append(new QueryFilterToken(tag.Value));
                return;
            }
            case IQueryFragmentNode.Arobase:
            {
                EatArobase();
                var author = EatData();
                _filterSettingBuilder.IllustratorName = new QueryFilterToken(author.Value);
                return;
            }
            case IQueryFragmentNode.A:
            {
                EatA();
                var author = EatData();
                _filterSettingBuilder.IllustratorName = new QueryFilterToken(author.Value);
                return;
            }
            case IQueryFragmentNode.C:
            {
                EatC();
                var character = EatData();
                _filterSettingBuilder.IncludeTags
                    = _filterSettingBuilder.IncludeTags.Append(new QueryFilterToken(character.Value));
                return;
            }
        }
    }

    public (long?, long?) ParseRangeDesc()
    {
        return CurrentFilterToken switch
        {
            IQueryFragmentNode.LeftBracket or IQueryFragmentNode.LeftParen => ParseIntervalForm(),
            IQueryFragmentNode.Dash or IQueryFragmentNode.Numeric => ParseDashForm(),
            _ => (null, null)
        };
    }

    public (long, long) ParseIntervalForm()
    {
        var leftInclusive = false;

        switch (CurrentFilterToken)
        {
            case IQueryFragmentNode.LeftBracket:
                EatLeftBracket();
                leftInclusive = true;
                break;
            case IQueryFragmentNode.LeftParen:
                EatLeftParen();
                leftInclusive = false;
                break;
        }


        var a = EatNumeric();
        EatComma();
        var b = EatNumeric();

        var rightInclusive = false;
        switch (CurrentFilterToken)
        {
            case IQueryFragmentNode.RightBracket:
                EatRightBracket();
                rightInclusive = true;
                break;
            case IQueryFragmentNode.RightParen:
                EatRightParen();
                rightInclusive = false;
                break;
        }

        return (leftInclusive, rightInclusive) switch
        {
            (true, true) => (a.Value, b.Value),
            (false, true) => (a.Value + 1, b.Value),
            (true, false) => (a.Value, b.Value - 1),
            (false, false) => (a.Value + 1, b.Value - 1)
        };
    }

    public (long?, long?) ParseDashForm()
    {
        switch (CurrentFilterToken)
        {
            case IQueryFragmentNode.Dash:
            {
                EatDash();
                var a = EatNumeric();
                return (null, a.Value);
            }
            case IQueryFragmentNode.Numeric:
            {
                var a = EatNumeric();
                EatDash();
                if (CurrentFilterToken is IQueryFragmentNode.Numeric)
                {
                    var b = EatNumeric();
                    return (a.Value, b.Value);
                }

                return (a.Value, null);
            }
            default:
                return (null, null);
        }
    }

    #region Lexical rules

    private IQueryFragmentNode.Data EatData()
    {
        var data = (IQueryFragmentNode.Data)Eat();
        return data;
    }

    private IQueryFragmentNode.Numeric EatNumeric()
    {
        var num = (IQueryFragmentNode.Numeric)Eat();
        return num;
    }

    private void EatHash()
    {
        var hash = (IQueryFragmentNode.Hashtag)Eat();
    }

    private void EatArobase()
    {
        var arobase = (IQueryFragmentNode.Arobase)Eat();
    }

    private void EatA()
    {
        var a = (IQueryFragmentNode.A)Eat();
    }

    private void EatC()
    {
        var c = (IQueryFragmentNode.C)Eat();
    }

    private void EatColon()
    {
        var colon = (IQueryFragmentNode.Colon)Eat();
    }

    private void EatComma()
    {
        var colon = (IQueryFragmentNode.Comma)Eat();
    }

    private void EatLeftBracket()
    {
        var lb = (IQueryFragmentNode.LeftBracket)Eat();
    }

    private void EatRightBracket()
    {
        var rb = (IQueryFragmentNode.RightBracket)Eat();
    }

    private void EatLeftParen()
    {
        var lp = (IQueryFragmentNode.LeftParen)Eat();
    }

    private void EatRightParen()
    {
        var rp = (IQueryFragmentNode.RightParen)Eat();
    }

    private void EatDash()
    {
        var dash = (IQueryFragmentNode.Dash)Eat();
    }

    #endregion
}

