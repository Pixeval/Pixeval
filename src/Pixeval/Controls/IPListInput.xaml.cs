using System.Collections.Generic;
using System.Net;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Header")]
[DependencyProperty<ICollection<string>>("ItemsSource")]
public sealed partial class IPListInput : StackPanel
{
    public IPListInput() => InitializeComponent();

    private void AddItem(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (ItemsSource.Contains(sender.Text))
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
        ItemsSource.Add(sender.Text);
    }

    private void TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
    {
        sender.BorderBrush = new SolidColorBrush(sender.Text is "" || IPAddress.TryParse(sender.Text, out _) ? Colors.Transparent : Colors.Red);
    }

    private void RemoveTapped(object sender, TappedRoutedEventArgs e) =>
        ItemsSource.Remove(sender.To<FrameworkElement>().GetTag<string>());
}
