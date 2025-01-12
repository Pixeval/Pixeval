// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

namespace Pixeval.CoreApi.Global.Exception;

/// <summary>
/// Raised if you're trying to set the sort option to popular_desc without a premium access
/// </summary>
public class IllegalSortOptionException : MakoException
{
    public IllegalSortOptionException()
    {
    }

    public IllegalSortOptionException(string? message) : base(message)
    {
    }

    public IllegalSortOptionException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}
