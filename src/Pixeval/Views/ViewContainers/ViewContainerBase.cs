// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace Pixeval.Views.ViewContainers;

public abstract class ViewContainerBase : ContentControl
{
    protected WindowNotificationManager Manager = null!;

    public abstract void NavigateTo<TParameter>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type pageType, object? icon, string header, TParameter parameter, bool removeCurrentPage = false);

    public void NavigateTo([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type pageType, object? icon, string header, bool removeCurrentPage = false) =>
        NavigateTo<object?>(pageType, icon, header, null, removeCurrentPage);

    public void NavigateTo<TPage, TParameter>(object? icon, string header, TParameter parameter, bool removeCurrentPage = false) where TPage : Control, new() =>
        NavigateTo(typeof(TPage), icon, header, parameter, removeCurrentPage);

    public void NavigateTo<TPage>(object? icon, string header, bool removeCurrentPage = false) where TPage : Control, new() =>
        NavigateTo<TPage, object?>(icon, header, null, removeCurrentPage);

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
