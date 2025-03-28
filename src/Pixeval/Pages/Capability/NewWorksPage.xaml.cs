// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class NewWorksPage : IScrollViewHost
{
    public NewWorksPage()
    {
        InitializeComponent();
        WorkTypeComboBox.SelectedEnum = App.AppViewModel.AppSettings.WorkType;
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => ChangeSource();

    private void WorkTypeComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(App.AppViewModel.MakoClient.NewWorks(WorkTypeComboBox.GetSelectedItem<WorkType>(), App.AppViewModel.AppSettings.TargetFilter));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;
}
