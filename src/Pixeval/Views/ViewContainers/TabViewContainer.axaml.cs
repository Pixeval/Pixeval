using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.Views.Login;
using Pixeval.Views.Viewers;
using Tabalonia.Controls;
using Tabalonia.InterTab;

namespace Pixeval.Views.ViewContainers;

public partial class TabViewContainer : ViewContainerBase
{
    public TabViewContainer()
    {
        InitializeComponent();

        TabsControl.InterTabController = new InterTabController
        {
            InterTabClient = new PixevalInterTabClient()
        };
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Manager = new WindowNotificationManager(TopLevel.GetTopLevel(this))
        {
            MaxItems = 3,
            Position = NotificationPosition.BottomRight
        };
    }

    /// <inheritdoc />
    public override void NavigateTo(
        Page page,
        bool removeCurrentPage = false)
    {
        var dragTabItem = new DragTabItem
        {
            Icon = page.Icon,
            Header = page.Header,
            Content = page
        };

        _ = TabsControl.Items.Add(dragTabItem);

        var selected = TabsControl.SelectedItem;
        TabsControl.SelectedItem = dragTabItem;

        if (removeCurrentPage)
            if (selected is not null)
                _ = TabsControl.Items.Remove(selected);
    }

    /// <summary>
    /// Custom <see cref="IInterTabClient"/> that creates new <see cref="Window"/>
    /// instances with a fresh <see cref="TabViewContainer"/> for tab tear-off.
    /// </summary>
    private sealed class PixevalInterTabClient : IInterTabClient
    {
        public TabHost GetNewHost(InterTabController controller, TabHost host)
        {
            var container = new TabViewContainer();
            var window = new Window
            {
                Content = container
            }.Fork(host.Window);

            return new TabHost(window, container.TabsControl);
        }
    }

    private void NavigationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: NavigationInfo { PageType: { } type } })
            return;

        // 默认参数是自己的Uid，无需指定
        NavigateTo((Page) Activator.CreateInstance(type)!);
    }

    private async void OpenMyPage_OnClick(object? sender, RoutedEventArgs e)
    {
        await this.CreateUserPageAsync(App.AppViewModel.PixivUid);
    }

    private void Logout_OnClicked(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this)?.ViewContainer?.NavigateTo(new LoginPage());
    }
}    
