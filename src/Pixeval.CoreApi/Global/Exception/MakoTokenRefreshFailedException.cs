// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

namespace Pixeval.CoreApi.Global.Exception;

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
