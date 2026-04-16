using Avalonia.Input;
using Avalonia.Interactivity;

namespace Pixeval.Views.Entry;

public partial class UserItem : EntryItem
{
    public UserItem() => InitializeComponent();

    private void AvatarButton_OnClicked(object? sender, RoutedEventArgs e) => e.Handled = true;

    private void AvatarButton_OnTapped(object? sender, TappedEventArgs e) => e.Handled = true;
}
