using System.Collections.ObjectModel;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Pixeval.Views.Settings;

public partial class IPListInput : StackPanel, IEntryControl<ISingleValueSettingsEntry<ObservableCollection<string>>>
{

    /// <inheritdoc />
    public ISingleValueSettingsEntry<ObservableCollection<string>> Entry { set => DataContext = value; }

    public IPListInput() => InitializeComponent();

    private void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ISingleValueSettingsEntry<ObservableCollection<string>> entry || IPv4Box.IPAddress is not { } ip)
            return;
        var value = ip.ToString();
        if (entry.Value.Contains(value))
        {
            ErrorInfoBar.IsVisible = true;
            return;
        }
        ErrorInfoBar.IsVisible = false;
        entry.Value.Add(value);
    }

    private void RemoveTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is ISingleValueSettingsEntry<ObservableCollection<string>> entry
            && sender is Control { DataContext: string s })
            _ = entry.Value.Remove(s);
    }
}
