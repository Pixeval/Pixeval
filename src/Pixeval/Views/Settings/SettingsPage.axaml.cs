using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.ViewModels;

namespace Pixeval.Views.Settings;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        _ = Frame.Navigate<SettingsMainView>();
    }

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        ValueSaving();
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    private void ValueSaving()
    {
        if (DataContext is not SettingsPageViewModel vm)
            return;
        foreach (var extensionGroup in vm.ExtensionGroups)
        foreach (var settingsEntry in extensionGroup)
            settingsEntry.ValueSaving(extensionGroup.Model.Values);
    }
}
