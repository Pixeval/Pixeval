// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;

namespace Pixeval.ViewModels;

public partial class LoginPageViewModel : ViewModelBase
{
    private readonly SemaphoreSlim _loadUsersLock = new(1, 1);
    private bool _areUsersLoaded;

    public LoginPageViewModel()
    {
        Users = [];
        RefreshToken = "";
    }

    public ObservableCollection<LoginUserEntry> Users { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedUser))]
    public partial LoginUserEntry? SelectedUser { get; set; }

    [ObservableProperty]
    public partial string RefreshToken { get; set; }

    [ObservableProperty]
    public partial bool IsLoginInProgress { get; set; }

    public bool HasSelectedUser => SelectedUser is not null;

    public async Task LoadUsersAsync(CancellationToken token = default)
    {
        await _loadUsersLock.WaitAsync(token);
        try
        {
            if (_areUsersLoaded)
                return;

            // Opening the login page must not depend on the database being available.
            var appViewModel = App.AppViewModel;
            var manager = appViewModel.AppServiceProvider.GetRequiredService<LoginUserPersistentManager>();
            var currentUserKey = appViewModel.LoginContext.CurrentKey;
            Users.Clear();
            await foreach (var user in manager.StreamEntriesAsync(token: token))
                Users.Add(user);
            _areUsersLoaded = true;
            SelectedUser = Users.FirstOrDefault(user => user.HistoryEntryId == currentUserKey);
        }
        finally
        {
            _ = _loadUsersLock.Release();
        }
    }

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
