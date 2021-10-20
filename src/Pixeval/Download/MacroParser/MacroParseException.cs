using System;

namespace Pixeval.Download.MacroParser
{
    public class MacroParseException : Exception
    {
        public MacroParseException(string? message) : base(message)
        {
        }
    }
}