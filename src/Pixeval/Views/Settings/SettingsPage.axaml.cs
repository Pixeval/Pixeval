// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
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
        ValueSaving(Parent);
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    private void ValueSaving(object? parent)
    {
        if (DataContext is not SettingsPageViewModel vm)
            return;

        foreach (var extensionGroup in vm.ExtensionGroups)
            foreach (var settingsEntry in extensionGroup)
                settingsEntry.ValueSaving(extensionGroup.Model.Values);

        AppInfo.SaveSettings();
        // Parent is TabsView
        TopLevel.GetTopLevel(parent as Control)?.ViewContainer?.ShowSuccess(I18NManager.GetResource(SettingsMainViewResources.SettingsSaved));
    }

    public static string ReleaseTitle => I18NManager.GetResource(SettingsMainViewResources.ReleaseNoteDialogTitleFormatted, AppInfo.AppVersion.CurrentVersionShortText);

    public static async Task<Control> GetReleaseNotesAsync()
    {
        await AppInfo.AppVersion.GitHubCheckForUpdateAsync();
        return new MarkdownBox
        {
            Markdown =
                AppInfo.AppVersion.CurrentAppReleaseModel?.ReleaseNote ??
                I18NManager.GetResource(SettingsMainViewResources.ReleaseNoteDialogEmpty)
        };
    }
}
