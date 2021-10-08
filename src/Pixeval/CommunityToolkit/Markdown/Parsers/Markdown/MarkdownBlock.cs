using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown
{
    /// <summary>
    /// A Block Element is an element that is a container for other structures.
    /// </summary>
    public abstract class MarkdownBlock : MarkdownElement
    {
        /// <summary>
        /// Gets or sets tells us what type this element is.
        /// </summary>
        public MarkdownBlockType Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownBlock"/> class.
        /// </summary>
        internal MarkdownBlock(MarkdownBlockType type)
        {
            Type = type;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <c>true</c> if the specified object is equal to the current object; otherwise, <c>false.</c> </returns>
        public override bool Equals(object? obj)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            if (!base.Equals(obj) || obj is not MarkdownBlock block)
            {
                return false;
            }

            return Type == block.Type;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        public override int GetHashCode()
        {
            // ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode() ^ Type.GetHashCode();
        }
    }
}