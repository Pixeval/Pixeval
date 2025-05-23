// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;
using Microsoft.UI.Xaml;

namespace Pixeval.Pages.Capability;

public sealed partial class IllustratorWorkPage : IScrollViewHost
{
    public IllustratorWorkPage()
    {
        InitializeComponent();
        WorkTypeComboBox.SelectedEnum = App.AppViewModel.AppSettings.WorkType;
    }

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

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}
