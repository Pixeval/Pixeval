using System;
using System.Runtime.Serialization;

namespace Pixeval.Objects.Exceptions
{
    public class QueryNotRespondingException : Exception
    {
        public QueryNotRespondingException()
        {
        }

        protected QueryNotRespondingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public QueryNotRespondingException(string message) : base(message)
        {
        }

        public QueryNotRespondingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}