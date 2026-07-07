// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.Views.Home;
using Pixeval.Views.Login;
using Pixeval.Views.Viewers;
using TabView.Avalonia;

namespace Pixeval.Views.ViewContainers;

public partial class TabViewContainer : ViewContainerBase
{
    public TabViewContainer()
    {
        InitializeComponent();

        AddHandler(
            ViewModelDisposal.ViewModelDisposalEvent,
            OnViewModelDisposal,
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        AddHandler(
            ViewModelDisposal.RequestDisposeEvent,
            OnRequestDispose,
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        Task.Delay(5000).ContinueWith(t => LoggingInDescriptionTextBlock.IsVisible = true, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void SetInterTabController(bool set)
    {
        TabsControl.InterTabController = set ? new InterTabController { InterTabClient = new PixevalInterTabClient() } : null;
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
        RegisterContentDialogHost(TopLevel.GetTopLevel(this));
    }

    /// <inheritdoc />
    public override void NavigateTo(
        Page page,
        bool removeCurrentPage = false)
    {
        if (TabsControl.Pages is not IList<Page> pages)
            throw new InvalidOperationException($"{nameof(TabsControl)} must use a mutable {nameof(TabsControl.Pages)} collection.");

        pages.Add(page);

        var selected = TabsControl.SelectedPage;
        TabsControl.SelectedIndex = pages.Count - 1;

        if (removeCurrentPage)
            if (selected is not null)
                _ = pages.Remove(selected);
    }

    private void NavigationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: NavigationInfo { PageType: { } type } })
            return;

        // 默认参数是自己的Uid，无需指定
        NavigateTo((Page) Activator.CreateInstance(type)!);
    }

    private void TabsView_OnAddTabButtonClick(TabsView sender, EventArgs e)
    {
        NavigateTo(new HomePage());
    }

    private void OpenMyPage_OnClick(object? sender, RoutedEventArgs e)
    {
        this.CreateUserPage(PixevalSettings.MyId);
    }

    private void SwitchAccount_OnClicked(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this)?.ViewContainer?.NavigateTo(new LoginPage());
    }

    private void TabsView_OnTabClosing(TabsView sender, TabClosingEventArgs e)
    {
        if (e.Item is not Control control
            || sender.Pages is not IReadOnlyCollection<Page> pages)
            return;
        // 关闭前手动切换标签页，以便触发标签页的 OnUnloaded 事件并保证其 Parent(TabsView) 存在，以便找 TopLevel(Window) 显示 InfoBar 之类的
        if (sender.SelectedIndex < pages.Count - 1)
            sender.SelectedIndex++;
        else if (sender.SelectedIndex > 0)
            sender.SelectedIndex--;
        // 否则关闭最后一个标签页，相当于关闭当前窗口，找到 TopLevel(Window) 也没意义

        var args = new RoutedEventArgs(ViewModelDisposal.RequestDisposeEvent, control);
        control.RaiseEvent(args);
        if (!args.Handled)
            DisposeTab(control);
    }

    private void OnViewModelDisposal(object? sender, ViewModelDisposalEventArgs e)
    {
        if (TabsControl.SelectedPage is not { } page)
            return;
        var set = ViewModelDisposal.DisposableMap.GetValueOrAddNew(page);
        set.Add(new(e.Disposable));
        e.Handled = true;
    }

    private static void OnRequestDispose(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Control control)
            return;
        DisposeTab(control);
        e.Handled = true;
    }

    private static void DisposeTab(Control tabItem)
    {
        if (!ViewModelDisposal.DisposableMap.Remove(tabItem, out var set))
            return;
        foreach (var disposable in set)
            if (disposable.TryGetTarget(out var target))
                target.Dispose();
        set.Clear();
    }

    /// <summary>
    /// Custom <see cref="IInterTabClient"/> that creates new <see cref="Window"/>
    /// instances with a fresh <see cref="TabViewContainer"/> for tab tear-off.
    /// </summary>
    private sealed class PixevalInterTabClient : IInterTabClient
    {
        public TabsHost GetNewHost(InterTabController controller, TabsHost host)
        {
            var container = new TabViewContainer();
            var window = new Window { Content = container }.Fork(host.Window);
            container.SetInterTabController(true);
            window.Closed += static (sender, args) =>
            {
                if (sender is TopLevel { ViewContainer: TabViewContainer container })
                    foreach (var page in container.TabsControl.Pages ?? [])
                        DisposeTab(page);
            };

            return new TabsHost(window, container.TabsControl);
        }
    }
}
