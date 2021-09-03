using System;
using System.Text;

namespace Pixeval.SourceGen
{
    public class IndentedStringBuilder
    {
        private readonly StringBuilder _stringBuilder = new();
        private readonly int _indentation;
        private int _indentationLevel;

        public IndentedStringBuilder(int indentation)
        {
            _indentation = indentation;
        }

        public IndentedStringBuilder PushIndent()
        {
            _indentationLevel++;
            return this;
        }

        public IndentedStringBuilder PopIndent()
        {
            if (--_indentationLevel < 0)
            {
                throw new IndexOutOfRangeException(nameof(_indentationLevel));
            }

            return this;
        }

        public IndentedStringBuilder Append(string s)
        {
            AppendIndent();
            _stringBuilder.Append(s);
            return this;
        }

        public IndentedStringBuilder Append(object o)
        {
            AppendIndent();
            _stringBuilder.Append(o);
            return this;
        }

        public IndentedStringBuilder AppendLine(string s)
        {
            AppendIndent();
            _stringBuilder.AppendLine(s);
            return this;
        }

        public IndentedStringBuilder AppendLine(object o)
        {
            AppendIndent();
            _stringBuilder.AppendLine(o.ToString());
            return this;
        }

        public BlockIndentedStringBuilder Block(string prefix, char open, char close)
        {
            Append(prefix + "\n" + open + "\n");
            return new BlockIndentedStringBuilder(this, close);
        }

        private void AppendIndent()
        {
            _stringBuilder.Append(new string(' ', _indentationLevel * _indentation));
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }

    public class BlockIndentedStringBuilder : IDisposable
    {
        private readonly IndentedStringBuilder _stringBuilder;
        private readonly char _close;

        public BlockIndentedStringBuilder(IndentedStringBuilder stringBuilder, char close)
        {
            _stringBuilder = stringBuilder;
            _close = close;
            PushIndent();
        }

        public IndentedStringBuilder PushIndent()
        {
            return _stringBuilder.PushIndent();
        }

        public IndentedStringBuilder PopIndent()
        {
            return _stringBuilder.PopIndent();
        }

        public IndentedStringBuilder Append(string s)
        {
            return _stringBuilder.Append(s);
        }

        public IndentedStringBuilder Append(object o)
        {
            return _stringBuilder.Append(o);
        }

        public IndentedStringBuilder AppendLine(string s)
        {
            return _stringBuilder.AppendLine(s);
        }

        public IndentedStringBuilder AppendLine(object o)
        {
            return _stringBuilder.AppendLine(o);
        }

        public BlockIndentedStringBuilder Block(string prefix, char open, char close)
        {
            return _stringBuilder.Block(prefix, open, close);
        }

        public void Dispose()
        {
            PopIndent();
            AppendLine(_close);
        }
    }
}