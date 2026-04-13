// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace Pixeval.Views.ViewContainers;

public abstract class ViewContainerBase : ContentControl
{
    protected WindowNotificationManager Manager = null!;

    public abstract void NavigateTo(Page page, bool removeCurrentPage = false);

    public void ShowNotification(INotification notification)
    {
        Manager.Show(notification);
    }

    public void ShowNotification(NotificationType type, string title, string? content = null)
    {
        ShowNotification(new Notification(title, content, type));
    }

    public void ShowInformation(string title, string? content = null) => ShowNotification(NotificationType.Information, title, content);

    public void ShowSuccess(string title, string? content = null) => ShowNotification(NotificationType.Success, title, content);

    public void ShowWarning(string title, string? content = null) => ShowNotification(NotificationType.Warning, title, content);

    public void ShowError(string title, string? content = null) => ShowNotification(NotificationType.Error, title, content);
}
