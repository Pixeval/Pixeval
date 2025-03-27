// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using Mako.Global.Enum;

namespace Mako.Global.Exception;

/// <summary>
/// When a <see cref="PrivacyPolicy" /> is set to <see cref="PrivacyPolicy.Private" /> while the uid is not equivalent
/// to the <see cref="MakoClient.Me" />
/// </summary>
public class IllegalPrivatePolicyException : MakoException
{
    public IllegalPrivatePolicyException(long uid)
    {
        Uid = uid;
    }

    public IllegalPrivatePolicyException(string? message, long uid) : base(message)
    {
        Uid = uid;
    }

    public IllegalPrivatePolicyException(string? message, System.Exception? innerException, long uid) : base(message, innerException)
    {
        Uid = uid;
    }

    public long Uid { get; }
}
