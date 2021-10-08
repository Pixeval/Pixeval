using System;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Core
{
    /// <summary>
    /// The StringValue attribute is used as a helper to decorate enum values with string representations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StringValueAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringValueAttribute"/> class.
        /// Constructor accepting string value.
        /// </summary>
        /// <param name="value">String value</param>
        public StringValueAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets property for string value.
        /// </summary>
        public string Value { get; }
    }
}