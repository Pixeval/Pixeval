// Copyright (c) Mako.
// Licensed under the GPL v3 License.

namespace Mako.Global.Exception;

public class MakoException : System.Exception
{
    public MakoException()
    {
    }

    public MakoException(string? message) : base(message)
    {
    }

    public MakoException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}
