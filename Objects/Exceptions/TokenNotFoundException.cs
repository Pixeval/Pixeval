using System;
using System.Runtime.Serialization;

namespace Pixeval.Objects.Exceptions
{
    public class TokenNotFoundException : Exception
    {
        public TokenNotFoundException()
        {
        }

        protected TokenNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TokenNotFoundException(string message) : base(message)
        {
        }

        public TokenNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}