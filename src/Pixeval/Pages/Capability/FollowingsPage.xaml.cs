// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Runtime;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class FollowingsPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public FollowingsPage() => InitializeComponent();

    private long _uid = -1;

    private bool IsMe => _uid == App.AppViewModel.PixivUid;

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (e.Parameter is not long uid)
            uid = App.AppViewModel.PixivUid;
        _uid = uid;
        ChangeSource();
    }

    private void PrivacyPolicyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        IllustratorView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Following(_uid, PrivacyPolicyComboBox.GetSelectedItem<PrivacyPolicy>()));
    }

    public ScrollView ScrollView => IllustratorView.ScrollView;

    public void CompleteDisposal()
    {
        Bindings.StopTracking();
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
