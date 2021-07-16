using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Pixeval
{
    /// <summary>
    /// Indicates that the login proxy failed to respond
    /// </summary>
    public class LoginProxyException : Exception
    {
        public LoginProxyException()
        {
        }

        protected LoginProxyException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public LoginProxyException([CanBeNull] string? message) : base(message)
        {
        }

        public LoginProxyException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException)
        {
        }
    }
}