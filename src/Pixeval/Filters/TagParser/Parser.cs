using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Filters.TagParser;

internal class Parser
{
    private List<QueryFragmentNode> Tokens { get; }

    private int Position { get; set; }

    private QueryFragmentNode Eat()
    {
        if (Position == Tokens.Count)
        {
            throw new Exception("FilterToken finished");
        }
        var tok = Tokens[Position];
        Position++;
        return tok;
    }

    private QueryFragmentNode CurrentFilterToken => this.Tokens[Position];

    private FilterSettingBuilder FilterSettingBuilder;

    public Parser(List<QueryFragmentNode> FilterTokens)
    {
        this.Position = 0;
        this.Tokens = FilterTokens;
        this.FilterSettingBuilder = new FilterSettingBuilder();
    }

    public FilterSetting Build()
    {
        ParseFilter();
        this.FilterSettingBuilder.IncludeTags = this.FilterSettingBuilder.IncludeTags.ToList();
        this.FilterSettingBuilder.ExcludeTags = this.FilterSettingBuilder.ExcludeTags.ToList();
        this.FilterSettingBuilder.ExcludeUserName = this.FilterSettingBuilder.ExcludeUserName.ToList();
        return this.FilterSettingBuilder.Build();
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

    // top-level entrypoint
    public void ParseFilter()
    {
        ParseArgumentList();
    }

    public void ParseArgumentList()
    {
        // If all tokens are eaten, return gracefully
        if (this.Position == this.Tokens.Count)
        {
            return;
        }
        // If this position is overflow, throw error
        if (this.Position > this.Tokens.Count)
        {
            throw new Exception("Expected an argument, actual: empty token flow");
        }
        // If the correct token is meet, call to parse argument
        if (this.CurrentFilterToken is QueryFragmentNode.Data or QueryFragmentNode.Hashtag or QueryFragmentNode.Arobase or QueryFragmentNode.A or QueryFragmentNode.C)
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
        if (this.CurrentFilterToken is QueryFragmentNode.Data)
        {
            var data = eatData();

            // The current parsing for `like` and `index` are retrieving them from Data token, it would be better to put them into dedicated tokens
            // in the current use case.
            if (data.data == "like")
            {
                eatColon();
                var (lo, hi) = ParseRangeDesc();

                if (lo is { } l)
                {
                    this.FilterSettingBuilder.LeastBookmark = l;
                }
                if (hi is { } h)
                {
                    this.FilterSettingBuilder.MaximumBookmark = h;
                }
            }
            else if (data.data == "index")
            {
                eatColon();
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
            }
            else
            {
                this.FilterSettingBuilder.IncludeTags
                    = this.FilterSettingBuilder.IncludeTags.Append(new QueryFilterToken(data.data));
            }
            return;
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.Hashtag)
        {
            eatHash();
            var tag = eatData();
            this.FilterSettingBuilder.IncludeTags
                = this.FilterSettingBuilder.IncludeTags.Append(new QueryFilterToken(tag.data));
            return;
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.Arobase)
        {
            eatArobase();
            var author = eatData();
            this.FilterSettingBuilder.IllustratorName = new QueryFilterToken(author.data);
            return;
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.A)
        {
            eatA();
            var author = eatData();
            this.FilterSettingBuilder.IllustratorName = new QueryFilterToken(author.data);
            return;
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.C)
        {
            eatC();
            var character = eatData();
            this.FilterSettingBuilder.IncludeTags
                = this.FilterSettingBuilder.IncludeTags.Append(new QueryFilterToken(character.data));
            return;
        }

    }

    public (long?, long?) ParseRangeDesc()
    {
        if (this.CurrentFilterToken is QueryFragmentNode.LeftBracket or QueryFragmentNode.LeftParen)
        {
            return ParseIntervalForm();
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.Dash or QueryFragmentNode.Numeric)
        {
            return ParseDashForm();
        }

        return (null, null);
    }

    public (long, long) ParseIntervalForm()
    {
        bool leftInclusive = false;

        if (this.CurrentFilterToken is QueryFragmentNode.LeftBracket)
        {
            eatLeftBracket();
            leftInclusive = true;
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.LeftParen)
        {
            eatLeftParen();
            leftInclusive = false;
        }


        var a = eatNumeric();
        eatComma();
        var b = eatNumeric();

        bool rightInclusive = false;
        if (this.CurrentFilterToken is QueryFragmentNode.RightBracket)
        {
            eatRightBracket();
            rightInclusive = true;
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.RightParen)
        {
            eatRightParen();
            rightInclusive = false;
        }

        switch (leftInclusive, rightInclusive)
        {
            case (true, true):
                return (a.value, b.value);
            case (false, true):
                return (a.value + 1, b.value);
            case (true, false):
                return (a.value, b.value - 1);
            case (false, false):
                return (a.value + 1, b.value - 1);
        }
    }

    public (long?, long?) ParseDashForm()
    {
        if (this.CurrentFilterToken is QueryFragmentNode.Dash)
        {
            eatDash();
            var a = eatNumeric();
            return (null, a.value);
        }
        else if (this.CurrentFilterToken is QueryFragmentNode.Numeric)
        {
            var a = eatNumeric();
            eatDash();
            if (this.CurrentFilterToken is QueryFragmentNode.Numeric)
            {
                var b = eatNumeric();
                return (a.value, b.value);
            }

            return (a.value, null);
        }

        return (null, null);
    }


    /*
     * Lexical rules
     */


    private QueryFragmentNode.Data eatData()
    {
        var data = (QueryFragmentNode.Data)Eat();
        return data;
    }

    private QueryFragmentNode.Numeric eatNumeric()
    {
        var num = (QueryFragmentNode.Numeric)Eat();
        return num;
    }

    private void eatHash()
    {
        var hash = (QueryFragmentNode.Hashtag)Eat();
    }

    private void eatArobase()
    {
        var arobase = (QueryFragmentNode.Arobase)Eat();
    }

    private void eatA()
    {
        var a = (QueryFragmentNode.A)Eat();
    }

    private void eatC()
    {
        var c = (QueryFragmentNode.C)Eat();
    }

    private void eatColon()
    {
        var colon = (QueryFragmentNode.Colon)Eat();
    }

    private void eatComma()
    {
        var colon = (QueryFragmentNode.Comma)Eat();
    }

    private void eatLeftBracket()
    {
        var lb = (QueryFragmentNode.LeftBracket)Eat();
    }

    private void eatRightBracket()
    {
        var rb = (QueryFragmentNode.RightBracket)Eat();
    }

    private void eatLeftParen()
    {
        var lp = (QueryFragmentNode.LeftParen)Eat();
    }

    private void eatRightParen()
    {
        var rp = (QueryFragmentNode.RightParen)Eat();
    }

    private void eatDash()
    {
        var dash = (QueryFragmentNode.Dash)Eat();
    }
}

