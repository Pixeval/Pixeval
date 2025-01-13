// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Runtime;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class IllustratorWorkPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public IllustratorWorkPage() => InitializeComponent();

    public ScrollView ScrollView => WorkContainer.ScrollView;

    private long _uid;

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        if (e.Parameter is not long id)
            return;
        _uid = id;
        ChangeSource();
    }

    private void WorkTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(App.AppViewModel.MakoClient.WorkPosts(_uid, WorkTypeComboBox.GetSelectedItem<WorkType>(), App.AppViewModel.AppSettings.TargetFilter));
    }

    public void CompleteDisposal()
    {
        Bindings.StopTracking();
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
