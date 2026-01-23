using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Mako.Global.Enum;
using Pixeval.Controls;

namespace Pixeval.Views.Capability;

public partial class RecentWorkPostsPage : UserControl
{
    public RecentWorkPostsPage()
    {
        InitializeComponent();
        AddHandler(Frame.NavigatedToEvent, (sender, e) => ChangeSource());
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        WorkContainer.ResetEngine(App.AppViewModel.MakoClient.RecentWorkPosts(
            SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>(),
            PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>()));
    }
}
