// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Runtime;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;

namespace Pixeval.Pages;

public sealed partial class MyPixivUsersPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public MyPixivUsersPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (e.Parameter is not long userId)
            userId = App.AppViewModel.PixivUid;

        IllustratorView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.MyPixivUsers(userId));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;

    public void CompleteDisposal()
    {
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
