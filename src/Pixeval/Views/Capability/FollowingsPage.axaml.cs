using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class FollowingsPage : UserControl
{
    private long _userId;

    public FollowingsPage()
    {
        InitializeComponent();
        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is not long uid)
                uid = App.AppViewModel.PixivUid;
            else if (uid != App.AppViewModel.PixivUid)
                PrivacyPolicyComboBox.IsEnabled = PrivacyPolicyComboBox.IsVisible = false;

            _userId = uid;
            ChangeSource();
        });
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
        (UserContainer.UserView.DataContext as UserViewViewModel)?.ResetEngine(App.AppViewModel.MakoClient.Following(_userId, PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>()));
    }
}
