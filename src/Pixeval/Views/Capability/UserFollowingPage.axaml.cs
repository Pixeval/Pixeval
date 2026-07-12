// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Interactivity;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class UserFollowingPage : IconContentPage
{
    private readonly long _userId;

    public UserFollowingPage() : this(PixevalSettings.MyId)
    {
    }

    public UserFollowingPage(long id, PrivacyPolicy privacyPolicy = PrivacyPolicy.Public, UserViewViewModel? viewModel = null)
    {
        InitializeComponent();
        _userId = id;
        PrivacyPolicyComboBox.SelectedValue = privacyPolicy;
        if (id != PixevalSettings.MyId)
            PrivacyPolicyComboBox.IsEnabled = PrivacyPolicyComboBox.IsVisible = false;
        if (viewModel is not null)
            UserContainer.UserView.SetViewModel(viewModel);
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
        ResetEngine(App.AppViewModel.MakoClient.UserFollowing(_userId, PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>()));
    }

    private void ResetEngine(IFetchEngine<User> fetchEngine) =>
        (UserContainer.UserView.DataContext as UserViewViewModel)?.ResetEngine(fetchEngine, static (user, _) => new(user));
}
