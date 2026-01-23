using AutoSettingsPage.Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.AppManagement;
using Pixeval.Models.Settings.Entries;

namespace Pixeval.Views.Settings;

public partial class DomainFrontingSettingsExpander : SettingsExpander, IEntryControl<DomainFrontingSettingsEntry<AppSettings>>
{
    public DomainFrontingSettingsEntry<AppSettings> Entry
    {
        set
        {
            DataContext = value;
            WrapPanel.Children.Clear();
            foreach (var entry in value.Entries)
                if (entry is IPSetSettingsEntry<AppSettings>)
                    WrapPanel.Children.Add(SettingsEntryHelper.GetControl(entry));
        }
    }

    public DomainFrontingSettingsExpander()
    {
        InitializeComponent();
    }
}
