// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class UserFollowingPage : ContentPage
{
    private readonly long _userId;

    public UserFollowingPage() : this(App.AppViewModel.PixivUid)
    {
    }

    public UserFollowingPage(long id, PrivacyPolicy privacyPolicy = PrivacyPolicy.Public, UserViewViewModel? viewModel = null)
    {
        InitializeComponent();
        _userId = id;
        PrivacyPolicyComboBox.SelectedValue = privacyPolicy;
        if (id != App.AppViewModel.PixivUid)
            PrivacyPolicyComboBox.IsEnabled = PrivacyPolicyComboBox.IsVisible = false;
        if (viewModel is not null)
        {
            var oldViewModel = UserContainer.UserView.DataContext as IDisposable;
            UserContainer.UserView.DataContext = viewModel;
            oldViewModel?.Dispose();
        }
        else
        {
            ChangeSource();
        }
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void UserContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        (UserContainer.UserView.DataContext as UserViewViewModel)?.ResetEngine(App.AppViewModel.MakoClient.UserFollowing(_userId, PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>()), (user, _) => new(user));
    }
}
