// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Runtime;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;

namespace Pixeval.Pages.Capability;

public sealed partial class SpotlightsPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public SpotlightsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        SpotlightView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Spotlights());
    }

    public ScrollView ScrollView => SpotlightView.ScrollView;

    public void CompleteDisposal()
    {
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
