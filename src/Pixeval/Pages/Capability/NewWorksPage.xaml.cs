using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Pages.Capability;

public sealed partial class NewWorksPage 
{
    public NewWorksPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e) => ChangeSource();

    private void WorkTypeComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(App.AppViewModel.MakoClient.NewWorks(WorkTypeComboBox.GetSelectedItem<WorkType>(), App.AppViewModel.AppSettings.TargetFilter));
    }
}
