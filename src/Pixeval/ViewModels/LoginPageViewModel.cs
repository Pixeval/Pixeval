// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;

namespace Pixeval.ViewModels;

public partial class LoginPageViewModel : ViewModelBase
{
    public LoginPageViewModel()
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<LoginUserPersistentManager>();
        Users = [.. manager.Reverse()];
        SelectedUser = manager.GetByKey(App.AppViewModel.LoginContext.CurrentKey);
        RefreshToken = SelectedUser?.RefreshToken ?? "";
    }

    public ObservableCollection<LoginUserEntry> Users { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedUser))]
    public partial LoginUserEntry? SelectedUser { get; set; }

    [ObservableProperty]
    public partial string RefreshToken { get; set; }

    public bool HasSelectedUser => SelectedUser is not null;

    public static AutoCompleteFilterPredicate<object> LoginUserFilter { get; } = static (_, item) => item is LoginUserEntry;

    public static AutoCompleteSelector<object> LoginUserTextSelector { get; } = static (_, item) =>
        item is LoginUserEntry user ? user.RefreshToken : item?.ToString() ?? "";

    partial void OnSelectedUserChanged(LoginUserEntry? value)
    {
        if (value is not null && RefreshToken != value.RefreshToken)
            RefreshToken = value.RefreshToken;
    }

    partial void OnRefreshTokenChanged(string value)
    {
        var selected = Users.FirstOrDefault(t => t.RefreshToken == value);
        if (!Equals(SelectedUser, selected))
            SelectedUser = selected;
    }
}
