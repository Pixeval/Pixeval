using System;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// An Exception that occurs when the Render Context is Incorrect.
    /// </summary>
    public class RenderContextIncorrectException : Exception
    {
        internal RenderContextIncorrectException()
            : base("Markdown Render Context missing or incorrect.")
        {
        }
    }
}