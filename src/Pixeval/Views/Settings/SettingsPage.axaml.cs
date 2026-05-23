// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.ViewModels;

namespace Pixeval.Views.Settings;

public partial class SettingsPage : NavigationPage
{
    public SettingsPage()
    {
        InitializeComponent();
        _ = PushAsync(new SettingsMainView());
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
