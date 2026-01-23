using Avalonia.Interactivity;

namespace Pixeval.Views.Entry;

public partial class UserItem : EntryItem
{
    public UserItem()
    {
        InitializeComponent();
    }

    private void AvatarButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }
}
