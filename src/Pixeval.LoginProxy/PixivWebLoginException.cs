using System;

namespace Pixeval.LoginProxy
{
    public enum Reason
    {
        ConnectToHostFailed = 1,
        CertificateNotFound = 2,
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