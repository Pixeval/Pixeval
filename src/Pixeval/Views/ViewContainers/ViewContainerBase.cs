// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Pixeval.Controls;
using Pixeval.I18N;

namespace Pixeval.Views.ViewContainers;

public abstract class ViewContainerBase : ContentControl
{
    protected WindowNotificationManager Manager = null!;
    protected ContentDialogHost DialogHost = null!;

    public abstract void NavigateTo(Page page, bool removeCurrentPage = false);

    protected void RegisterContentDialogHost(TopLevel? topLevel)
    {
        if (topLevel is not null)
            DialogHost = new(topLevel);
    }

    public Task<ContentDialogResult> ShowContentDialogAsync(ContentDialog dialog) =>
        DialogHost.ShowAsync(dialog);

    public Task<ContentDialogResult> CreateOkCancelAsync(string title, string content) =>
        ShowContentDialogAsync(new()
        {
            Title = title,
            Content = content,
            PrimaryButtonText = I18NManager.GetResource(ContentDialogResources.OkButtonContent),
            CloseButtonText = I18NManager.GetResource(ContentDialogResources.CancelButtonContent),
            DefaultButton = ContentDialogButton.Primary
        });

    public Task<ContentDialogResult> CreateAcknowledgementAsync(string title, string? content) =>
        ShowContentDialogAsync(new()
        {
            Title = title,
            Content = content,
            PrimaryButtonText = I18NManager.GetResource(ContentDialogResources.OkButtonContent),
            DefaultButton = ContentDialogButton.Primary
        });

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
