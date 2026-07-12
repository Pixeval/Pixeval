// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class WorkFollowingPage : IconContentPage
{
    public WorkFollowingPage() : this(PixevalSettings.SimpleWorkType, PrivacyPolicy.Public)
    {
    }

    public WorkFollowingPage(SimpleWorkType simpleWorkType, PrivacyPolicy privacyPolicy, IWorkViewViewModel? viewModel = null)
    {
        InitializeComponent();
        SimpleWorkTypeComboBox.SelectedValue = simpleWorkType;
        PrivacyPolicyComboBox.SelectedValue = privacyPolicy;
        if (viewModel is not null)
            WorkContainer.SetViewModel(viewModel);
        else
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
