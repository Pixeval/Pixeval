using System;

namespace Pixeval.LoginProxy
{
    public enum Reason
    {
        ConnectToHostFailed,
        CertificateNotFound
    }

    public class PixivWebLoginException : Exception
    {
        public PixivWebLoginException(Reason reason)
        {
            Reason = reason;
        }

        public Reason Reason { get; }
    }
}