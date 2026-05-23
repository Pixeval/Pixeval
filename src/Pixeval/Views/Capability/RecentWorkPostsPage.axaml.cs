// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Pixeval.Controls;

namespace Pixeval.Views.Capability;

public partial class RecentWorkPostsPage : ContentPage
{
    public RecentWorkPostsPage()
    {
        InitializeComponent();
        ChangeSource();
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
        WorkContainer.ResetEngine(App.AppViewModel.MakoClient.WorkFollowing(
            SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>(),
            PrivacyPolicyComboBox.GetSelectedValue<PrivacyPolicy>()));
    }
}
