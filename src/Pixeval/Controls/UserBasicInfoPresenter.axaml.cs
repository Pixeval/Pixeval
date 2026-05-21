// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls.Primitives;
using Mako.Model;
using Pixeval.AppManagement;

namespace Pixeval.Controls;

public class UserBasicInfoPresenter : TemplatedControl
{
    public static readonly StyledProperty<UserBasicInfo?> UserProperty =
        AvaloniaProperty.Register<UserBasicInfoPresenter, UserBasicInfo?>(nameof(User));

    public static readonly DirectProperty<UserBasicInfoPresenter, string> AvatarUrlProperty =
        AvaloniaProperty.RegisterDirect<UserBasicInfoPresenter, string>(nameof(AvatarUrl), o => o.AvatarUrl);

    public static readonly DirectProperty<UserBasicInfoPresenter, string> UserDisplayNameProperty =
        AvaloniaProperty.RegisterDirect<UserBasicInfoPresenter, string>(nameof(UserDisplayName), o => o.UserDisplayName);

    public static readonly DirectProperty<UserBasicInfoPresenter, string> AccountDisplayProperty =
        AvaloniaProperty.RegisterDirect<UserBasicInfoPresenter, string>(nameof(AccountDisplay), o => o.AccountDisplay);

    static UserBasicInfoPresenter()
    {
        UserProperty.Changed.AddClassHandler<UserBasicInfoPresenter>(static (control, e) =>
        {
            control.UpdateFromUser(e.GetNewValue<UserBasicInfo?>());
        });
    }

    public UserBasicInfoPresenter()
    {
        UpdateFromUser(User);
    }

    public UserBasicInfo? User
    {
        get => GetValue(UserProperty);
        set => SetValue(UserProperty, value);
    }

    public string AvatarUrl
    {
        get;
        private set => SetAndRaise(AvatarUrlProperty, ref field, value);
    } = AppInfo.PixivNoProfilePath;

    public string UserDisplayName
    {
        get;
        private set => SetAndRaise(UserDisplayNameProperty, ref field, value);
    } = "";

    public string AccountDisplay
    {
        get;
        private set => SetAndRaise(AccountDisplayProperty, ref field, value);
    } = "";

    private void UpdateFromUser(UserBasicInfo? user)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(user?.AvatarUrl)
            ? AppInfo.PixivNoProfilePath
            : user.AvatarUrl;
        UserDisplayName = user is null
            ? ""
            : string.IsNullOrWhiteSpace(user.Name) ? user.Id.ToString() : user.Name;
        AccountDisplay = string.IsNullOrWhiteSpace(user?.Account) ? "" : $"@{user.Account}";
    }
}
