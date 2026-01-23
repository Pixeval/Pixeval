using System;
using System.Diagnostics.CodeAnalysis;
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
    public override void NavigateTo<TParameter>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type pageType,
        object? icon,
        string header,
        TParameter parameter,
        bool removeCurrentPage = false)
    {
        _ = Frame.Navigate(pageType, parameter);
        if (removeCurrentPage)
            if (Frame.BackStack.Count > 0)
                Frame.BackStack.RemoveAt(0);
    }
}
