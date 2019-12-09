using System;
using System.Runtime.Serialization;

namespace Pixeval.Objects.Exceptions
{
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException()
        {
        }

        protected TypeMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TypeMismatchException(string message) : base(message)
        {
        }

        public TypeMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}