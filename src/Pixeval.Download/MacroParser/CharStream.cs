// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Download.MacroParser;

public class CharStream : ISeekable<char>
{
    private readonly Stack<int> _markers = new();
    private readonly string _text;

    private ReadOnlySpan<char> StreamSpan => _text.AsSpan();

    public CharStream(string text)
    {
        _text = text;
        _markers.Push(0);
    }

    public int Forward { get; private set; }

    public void Seek(int pos)
    {
        _ = _markers.Pop();
        _markers.Push(pos);
        Forward = pos;
    }

    public char Peek()
    {
        return Forward >= StreamSpan.Length ? char.MaxValue : StreamSpan[Forward];
    }

    public void Advance()
    {
        Forward++;
    }

    public void Advance(int n)
    {
        Forward += n;
    }

    public ReadOnlySpan<char> GetWindow()
    {
        return StreamSpan[_markers.Peek()..Forward];
    }

    public void AdvanceMarker()
    {
        _ = _markers.Pop();
        _markers.Push(Forward);
    }

    public void ResetForward()
    {
        Forward = _markers.Peek();
    }

    public void Return()
    {
        Forward--;
    }

    public static async Task<CharStream> Load(Stream stream, Encoding encoding)
    {
        using var sr = new StreamReader(stream, encoding);
        return new(await sr.ReadToEndAsync());
    }

    public LineInfo GetCurrentLineInfo()
    {
        if (Forward >= _text.Length)
        {
            return LineInfo.Eof;
        }

        var lines = _text[..Forward].Split(Environment.NewLine);
        return new(lines.Length, lines[^1].Length);
    }

    public char NextChar()
    {
        var c = Peek();
        Forward++;
        return c;
    }

    public void Return(int count)
    {
        Forward -= count;
    }

    public int UntilAndReturn(char c)
    {
        var cnt = 0;
        while (Peek() == c)
        {
            Advance();
            cnt++;
        }

        Return();
        return cnt;
    }

    public int Until(char c)
    {
        var cnt = 0;
        while (Peek() == c)
        {
            Advance();
            cnt++;
        }

        return cnt;
    }

    public bool UntilLimited(char c, int limit)
    {
        var cnt = 0;
        while (Peek() == c)
        {
            if (++cnt > limit)
            {
                return false;
            }

            Advance();
        }

        return true;
    }

    public bool UntilLimitedIf(Func<char, bool> func, int limit)
    {
        var cnt = 0;
        while (func(Peek()))
        {
            if (++cnt > limit)
            {
                return false;
            }

            Advance();
        }

        return true;
    }

    public string GetUntilIf(Func<char, bool> func)
    {
        while (func(Peek()) && Peek() is not char.MaxValue)
        {
            Advance();
        }

        return new(GetWindow());
    }

    public void PushMarker()
    {
        _markers.Push(Forward);
    }

    public int PopMarker()
    {
        return _markers.Pop();
    }

    public string GetUntilIfAndReturn(Func<char, bool> func)
    {
        var str = GetUntilIf(func);
        Return();
        return str;
    }

    public struct LineInfo(int lineNumber, int position)
    {
        public static readonly LineInfo Eof = new(-1, -1);

        public int LineNumber = lineNumber;

        public int Position = position;
    }
}
