using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Filters.TagParser;

public class Parser
{
    private List<IQueryFragmentNode> Tokens { get; }
    
    private ITokenTreeNode QueryTokenTree;

    private ITokenTreeNode CurrentTopLevel;

    public Parser(List<IQueryFragmentNode> filterTokens)
    {
        this.Tokens = filterTokens;
        this.QueryTokenTree = new TokenOrNode(new List<ITokenTreeNode>());
        this.CurrentTopLevel = this.QueryTokenTree;
    }

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

    public ITokenTreeNode Build()
    {
        return this.QueryTokenTree;
    }

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
        ParseArgumentList();
    }

    public void ParseArgument()
    {
        switch (CurrentFilterToken)
        {
            // And/Or
            case IQueryFragmentNode.LeftParen:
            {
                EatLeftParen();
                if (CurrentFilterToken is IQueryFragmentNode.And)
                {
                    var nextAndNode = new TokenAndNode(new List<ITokenTreeNode>());
                    this.CurrentTopLevel.Insert(nextAndNode);
                    this.CurrentTopLevel = nextAndNode;
                    
                    ParseAndList();
                    
                    if (CurrentTopLevel.Parent != null)
                    {
                        this.CurrentTopLevel = CurrentTopLevel.Parent;
                    }
                    
                } else if (CurrentFilterToken is IQueryFragmentNode.Or)
                {
                    var nextOrNode = new TokenOrNode(new List<ITokenTreeNode>());
                    this.CurrentTopLevel.Insert(nextOrNode);
                    this.CurrentTopLevel = nextOrNode;
                    
                    ParseAndList();
                    
                    if (CurrentTopLevel.Parent != null)
                    {
                        this.CurrentTopLevel = CurrentTopLevel.Parent;
                    }
                }
                else
                {
                    throw new Exception("Expected either and/or token after eating left paren");
                }

                return;
            }
            // Data or keyword
            case IQueryFragmentNode.Data:
            {
                var data = EatData();

                var tagToken = new TagToken(data.Value);
                CurrentTopLevel.Insert(tagToken);

                return;
            }
            case IQueryFragmentNode.Hashtag:
            {
                EatHash();
                var data = EatData();
                
                var tagToken = new TagToken(data.Value);
                CurrentTopLevel.Insert(tagToken);
                
                return;
            }
            case IQueryFragmentNode.Arobase:
            {
                EatArobase();
                var author = EatData();
                
                var authorTagToken = new TagToken(author.Value);
                CurrentTopLevel.Insert(authorTagToken);
                
                return;
            }
            case IQueryFragmentNode.A:
            {
                EatA();
                EatColon();
                var author = EatData();
                
                var authorTagToken = new TagToken(author.Value);
                CurrentTopLevel.Insert(authorTagToken);
                
                return;
            }
            case IQueryFragmentNode.C:
            {
                EatC();
                EatColon();
                var character = EatData();
                
                var characterTagToken = new TagToken(character.Value);
                CurrentTopLevel.Insert(characterTagToken);
                
                return;
            }
            case IQueryFragmentNode.E:
            {
                EatC();
                EatColon();
                var (year, month, day) = ParseDateDesc();

                var date = new DateTime(year ?? DateTime.Today.Year, month, day);

                var dateOffset = new DateTimeOffset(date);
                
                var dateToken = new DateToken(RangeEdge.Ending, dateOffset);
                CurrentTopLevel.Insert(dateToken);
                
                return;
            }
            case IQueryFragmentNode.L:
            {
                EatC();
                EatColon();
                var (start, end) = ParseRangeDesc();

                var rangeToken = new NumericRangeToken(RangeType.Collection, start, end);
                CurrentTopLevel.Insert(rangeToken);
                
                return;
            }
            case IQueryFragmentNode.N:
            {
                EatC();
                EatColon();
                var (start, end) = ParseRangeDesc();

                var rangeToken = new NumericRangeToken(RangeType.Sequences, start, end);
                CurrentTopLevel.Insert(rangeToken);
                
                return;
            }
            case IQueryFragmentNode.S:
            {
                EatC();
                EatColon();
                var (year, month, day) = ParseDateDesc();

                var date = new DateTime(year ?? DateTime.Today.Year, month, day);

                var dateOffset = new DateTimeOffset(date);
                
                var dateToken = new DateToken(RangeEdge.Starting, dateOffset);
                CurrentTopLevel.Insert(dateToken);
                
                return;
            }
        }
    }

    public void ParseAndList()
    {
        EatAnd();
        ParseArgumentList();
        EatRightParen();
    }

    public void ParseOrList()
    {
        EatOr();
        ParseArgumentList();
        EatRightParen();
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

    public (int?, int, int) ParseDateDesc()
    {
        var firstPart = EatNumeric();
        if (CurrentFilterToken is IQueryFragmentNode.Dash or IQueryFragmentNode.Dot)
        {
            var secondPart = EatNumeric();

            if (CurrentFilterToken is IQueryFragmentNode.Dash or IQueryFragmentNode.Dot)
            {
                var thirdPart = EatNumeric();

                return ((int) firstPart.Value, (int) secondPart.Value, (int) thirdPart.Value);
            }
            else
            {
                return (null, (int) firstPart.Value, (int) secondPart.Value);
            }
        }
        else
        {
            throw new Exception("At least two numeric parts should be contained in a date segment");
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

    private void EatAnd()
    {
        var and = (IQueryFragmentNode.And)Eat();
    }
    
    private void EatOr()
    {
        var or = (IQueryFragmentNode.And)Eat();
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
    
    private void EatE()
    {
        var c = (IQueryFragmentNode.E)Eat();
    }
    
    private void EatL()
    {
        var c = (IQueryFragmentNode.L)Eat();
    }
    
    private void EatN()
    {
        var c = (IQueryFragmentNode.N)Eat();
    }
    
    private void EatS()
    {
        var c = (IQueryFragmentNode.S)Eat();
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

