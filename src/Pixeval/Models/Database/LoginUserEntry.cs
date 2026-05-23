// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using Pixeval.AppManagement;
using SQLite;

namespace Pixeval.Models.Database;

public class LoginUserEntry : HistoryEntry
{
    [Indexed(Unique = true)]
    public string RefreshToken { get; set; } = "";

    [Indexed(Unique = true)]
    public long UserId { get; set; }

    public string Name { get; set; } = "";

    public string Account { get; set; } = "";

    public string MailAddress { get; set; } = "";

    public bool IsPremium { get; set; }

    public XRestrict XRestrict { get; set; }

    public bool IsMailAuthorized { get; set; }

    public bool RequirePolicyAgreement { get; set; }

    public string Avatar16Url { get; set; } = AppInfo.PixivNoProfilePath;

    public string Avatar50Url { get; set; } = AppInfo.PixivNoProfilePath;

    public string Avatar170Url { get; set; } = AppInfo.PixivNoProfilePath;

    [Ignore]
    public string AvatarUrl => string.IsNullOrWhiteSpace(Avatar50Url) ? AppInfo.PixivNoProfilePath : Avatar50Url;

    [Ignore]
    public string AccountDisplay => string.IsNullOrWhiteSpace(Account) ? "" : $"@{Account}";

    [Ignore]
    public TokenUser TokenUser => new()
    {
        Id = UserId,
        Name = Name,
        Account = Account,
        ProfileImageUrls = new()
        {
            Px16X16 = Avatar16Url,
            Px50X50 = Avatar50Url,
            Px170X170 = Avatar170Url
        },
        MailAddress = MailAddress,
        IsPremium = IsPremium,
        XRestrict = XRestrict,
        IsMailAuthorized = IsMailAuthorized,
        RequirePolicyAgreement = RequirePolicyAgreement
    };

    public static LoginUserEntry FromTokenUser(string refreshToken, TokenUser user)
    {
        return new()
        {
            RefreshToken = refreshToken,
            UserId = user.Id,
            Name = user.Name,
            Account = user.Account,
            MailAddress = user.MailAddress,
            IsPremium = user.IsPremium,
            XRestrict = user.XRestrict,
            IsMailAuthorized = user.IsMailAuthorized,
            RequirePolicyAgreement = user.RequirePolicyAgreement,
            Avatar16Url = user.ProfileImageUrls.Px16X16,
            Avatar50Url = user.ProfileImageUrls.Px50X50,
            Avatar170Url = user.ProfileImageUrls.Px170X170
        };
    }

    public void UpdateFrom(LoginUserEntry entry)
    {
        RefreshToken = entry.RefreshToken;
        UserId = entry.UserId;
        Name = entry.Name;
        Account = entry.Account;
        MailAddress = entry.MailAddress;
        IsPremium = entry.IsPremium;
        XRestrict = entry.XRestrict;
        IsMailAuthorized = entry.IsMailAuthorized;
        RequirePolicyAgreement = entry.RequirePolicyAgreement;
        Avatar16Url = entry.Avatar16Url;
        Avatar50Url = entry.Avatar50Url;
        Avatar170Url = entry.Avatar170Url;
    }

    public override string ToString() => RefreshToken;
}
