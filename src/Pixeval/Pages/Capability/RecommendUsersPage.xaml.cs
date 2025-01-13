// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Runtime;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendUsersPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public RecommendUsersPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        IllustratorView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.RecommendIllustrators());
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;

    public void CompleteDisposal()
    {
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
