using System;
using System.Runtime.Serialization;

namespace Pzxlane.Objects.Exceptions
{
    public class RetryLimitException : Exception
    {
        public RetryLimitException()
        {
        }

        protected RetryLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RetryLimitException(string message) : base(message)
        {
        }

        public RetryLimitException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}