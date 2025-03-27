// Copyright (c) Mako.
// Licensed under the GPL v3 License.

namespace Mako.Global.Exception;

public class MakoTokenRefreshFailedException : MakoException
{
    public MakoTokenRefreshFailedException()
    {
    }

    public MakoTokenRefreshFailedException(string? message) : base(message)
    {
    }

    public MakoTokenRefreshFailedException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}
