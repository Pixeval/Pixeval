using System;

namespace Pixeval.Download.MacroParser.Ast
{
    public class IllegalMacroException : Exception
    {
        public IllegalMacroException(string? message) : base(message)
        {
        }
    }
}