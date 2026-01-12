// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.ObjectModel;
using System.Net;
using AutoSettingsPage.Models;
using AutoSettingsPage.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class IPListInput : StackPanel, IEntryControl<ISingleValueSettingsEntry<ObservableCollection<string>>>
{
    /// <inheritdoc />
    public ISingleValueSettingsEntry<ObservableCollection<string>> Entry { get; set; } = null!;

    public IPListInput() => InitializeComponent();

    private void AddItem(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (Entry.Value.Contains(sender.Text))
        {
            ErrorInfoBar.Severity = InfoBarSeverity.Warning;
            ErrorInfoBar.Message = IPListInputResources.DuplicatesWithExistingIP;
            ErrorInfoBar.IsOpen = true;
            return;
        }

        if (!IPAddress.TryParse(sender.Text, out _))
        {
            ErrorInfoBar.Severity = InfoBarSeverity.Error;
            ErrorInfoBar.Message = IPListInputResources.InvalidIPFormat;
            ErrorInfoBar.IsOpen = true;
            return;
        }

        ErrorInfoBar.IsOpen = false;
        Entry.Value.Add(sender.Text);
    }

    private void TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
    {
        sender.BorderBrush = new SolidColorBrush(sender.Text is "" || IPAddress.TryParse(sender.Text, out _) ? Colors.Transparent : Colors.Red);
    }

    private void RemoveTapped(object sender, TappedRoutedEventArgs e) =>
        Entry.Value.Remove(sender.To<FrameworkElement>().GetTag<string>());
}
