// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;
using Microsoft.UI.Xaml;
using Pixeval.Util;
using WinRT;
using Mako.Model;

namespace Pixeval.Pages.Capability;

public sealed partial class RankingsPage : IScrollViewHost
{
    public RankingsPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        ChangeSource();
    }

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ChangeSource();
    }

    private void OnSelectionChanged(object sender, IWinRTObject e) => ChangeSource();

    private async void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(await App.AppViewModel.GetEngineAsync<Illustration>("ranking"));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}
