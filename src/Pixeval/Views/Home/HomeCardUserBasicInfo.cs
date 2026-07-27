// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Diagnostics.CodeAnalysis;
using Mako.Model;
using Pixeval.AppManagement;

namespace Pixeval.Views.Home;

public sealed record HomeCardUserBasicInfo : UserBasicInfo
{
    [SetsRequiredMembers]
    public HomeCardUserBasicInfo(long userId, string? userName)
    {
        Id = userId;
        Name = string.IsNullOrWhiteSpace(userName) ? userId.ToString() : userName;
        Account = "";
    }

    public override string AvatarUrl => AppInfo.ImageNotAvailablePath;
}
