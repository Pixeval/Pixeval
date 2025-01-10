// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class IllustratorWorkPage : IScrollViewHost
{
    public IllustratorWorkPage() => InitializeComponent();

    public ScrollView ScrollView => WorkContainer.ScrollView;

    private long _uid;

    public override void OnPageActivated(NavigationEventArgs e)
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
}
