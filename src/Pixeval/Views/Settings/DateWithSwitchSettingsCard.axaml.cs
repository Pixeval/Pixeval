using AutoSettingsPage.Avalonia;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.AppManagement;
using Pixeval.Models.Settings.Entries;

namespace Pixeval.Views.Settings;

public partial class DateWithSwitchSettingsCard : SettingsCard, IEntryControl<DateWithSwitchSettingsEntry<AppSettings>>
{
    public DateWithSwitchSettingsEntry<AppSettings> Entry { set => DataContext = value; }

    public DateWithSwitchSettingsCard() => InitializeComponent();
}
