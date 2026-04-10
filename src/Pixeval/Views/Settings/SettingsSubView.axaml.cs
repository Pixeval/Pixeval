using AutoSettingsPage.Models;
using Avalonia.Controls;

namespace Pixeval.Views.Settings;

public partial class SettingsSubView : ContentPage
{
    public SettingsSubView(ISettingsGroup group)
    {
        InitializeComponent();
        Header = group.Header;
        ItemsControl.ItemsSource = group;
    }
}
