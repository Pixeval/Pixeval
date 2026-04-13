using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class FollowingsPage : ContentPage
{
    private readonly long _userId;

    public FollowingsPage() : this(App.AppViewModel.PixivUid)
    {
    }

    public FollowingsPage(long id)
    {
        InitializeComponent();
        _userId = id;
        if (id != App.AppViewModel.PixivUid)
            PrivacyPolicyComboBox.IsEnabled = PrivacyPolicyComboBox.IsVisible = false;
        ChangeSource();
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
        (UserContainer.UserView.DataContext as UserViewViewModel)?.ResetEngine(App.AppViewModel.MakoClient.Following(_userId, PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>()), (user, _) => new(user));
    }
}
