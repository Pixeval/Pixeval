using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixeval.Download.MacroParser
{
    public class CharStream : ISeekable<char>
    {
        private readonly Stack<int> markers = new();
        private readonly string text;

        private char[] stream;

        public int Forward { get; private set; }

        public CharStream(string text)
        {
            stream = text.ToCharArray();
            this.text = text;
            markers.Push(0);
        }

        public void Seek(int pos)
        {
            markers.Pop();
            markers.Push(pos);
            Forward = pos;
        }

        public char Peek()
        {
            return Forward >= stream.Length ? char.MaxValue : stream[Forward];
        }

        public void Advance()
        {
            Forward++;
        }

        public void Advance(int n)
        {
            Forward += n;
        }

        public char[] GetWindow()
        {
            return stream[markers.Peek()..Forward];
        }

        public void AdvanceMarker()
        {
            markers.Pop();
            markers.Push(Forward);
        }

        public void ResetForward()
        {
            Forward = markers.Peek();
        }

        public void Return()
        {
            Forward--;
        }

        public static async Task<CharStream> Load(Stream stream, Encoding encoding)
        {
            using var sr = new StreamReader(stream, encoding);
            return new CharStream(await sr.ReadToEndAsync());
        }

        public LineInfo GetCurrentLineInfo()
        {
            if (Forward >= text.Length)
            {
                return LineInfo.Eof;
            }

            var lines = text[..Forward].Split(Environment.NewLine);
            return new LineInfo(lines.Length, lines[^1].Length);
        }

        public void Replace(char[] newStream)
        {
            stream = newStream;
            Forward = 0;
            if (markers.Any())
            {
                markers.Clear();
            }

            markers.Push(0);
        }

        public char NextChar()
        {
            var c = Peek();
            Forward++;
            return c;
        }

        public string GetWindowString()
        {
            return new string(stream[markers.Peek()..Forward]);
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

            return new string(GetWindow());
        }

        public void PushMarker()
        {
            markers.Push(Forward);
        }

        public int PopMarker()
        {
            return markers.Pop();
        }

        public string GetUntilIfAndReturn(Func<char, bool> func)
        {
            var str = GetUntilIf(func);
            Return();
            return str;
        }

        public struct LineInfo
        {
            public static readonly LineInfo Eof = new(-1, -1);

            public int LineNumber;

            public int Position;

            public LineInfo(int lineNumber, int position)
            {
                LineNumber = lineNumber;
                Position = position;
            }
        }
    }
}