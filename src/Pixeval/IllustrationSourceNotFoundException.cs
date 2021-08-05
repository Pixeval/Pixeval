using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Pixeval
{
    public class IllustrationSourceNotFoundException : Exception
    {
        public IllustrationSourceNotFoundException()
        {
        }

        protected IllustrationSourceNotFoundException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IllustrationSourceNotFoundException([CanBeNull] string? message) : base(message)
        {
        }

        public IllustrationSourceNotFoundException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException)
        {
        }
    }
}