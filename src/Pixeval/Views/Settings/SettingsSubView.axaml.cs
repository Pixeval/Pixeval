using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;

namespace Pixeval.Views.Settings;

public partial class SettingsSubView : UserControl
{
    public SettingsSubView()
    {
        InitializeComponent();
        AddHandler(Frame.NavigatedToEvent, (sender, e) =>
        {
            if (e.Parameter is not ISettingsGroup group)
                return;
            HeaderTextBlock.Text = group.Header;
            ItemsControl.ItemsSource = group;
        });
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Parent is Frame frame)
            frame.GoBack();
    }
}
