// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Runtime;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public sealed partial class SearchUsersPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public SearchUsersPage() => InitializeComponent();

    private string _searchText = "";

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _searchText = e.Parameter.To<string>();
        ChangeSource();
    }

    private void ChangeSource()
    {
        var settings = App.AppViewModel.AppSettings;
        IllustratorView.ViewModel.ResetEngine(
            App.AppViewModel.MakoClient.SearchUser(
                _searchText,
                settings.TargetFilter));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;

    public void CompleteDisposal()
    {
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
