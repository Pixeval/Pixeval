// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Runtime;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class RecentPostsPage : IScrollViewHost, IStructuralDisposalCompleter
{
    public RecentPostsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var privacyPolicy = PrivacyPolicyComboBox.GetSelectedItem<PrivacyPolicy>();
        WorkContainer.WorkView.ResetEngine(SimpleWorkTypeComboBox.GetSelectedItem<SimpleWorkType>() is SimpleWorkType.IllustAndManga
            ? App.AppViewModel.MakoClient.RecentIllustrationPosts(privacyPolicy)
            : App.AppViewModel.MakoClient.RecentNovelPosts(privacyPolicy));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;

    public void CompleteDisposal()
    {
        Bindings.StopTracking();
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
