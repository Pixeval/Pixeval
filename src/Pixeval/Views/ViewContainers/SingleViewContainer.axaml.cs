using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;

namespace Pixeval.Views.ViewContainers;

public partial class SingleViewContainer : ViewContainerBase
{
    public SingleViewContainer()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Manager = new WindowNotificationManager(TopLevel.GetTopLevel(this))
        {
            MaxItems = 1,
            Position = NotificationPosition.BottomCenter
        };
    }

    /// <inheritdoc />
    public override async void NavigateTo(Page page, bool removeCurrentPage = false)
    {
        await Frame.PopAsync();
        await Frame.PushAsync(page);
    }
}
