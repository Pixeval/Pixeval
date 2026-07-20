// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using CommunityToolkit.Mvvm.Input;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Navigation;
using Pixeval.Utilities;
using Pixeval.Views.Login;
using Pixeval.Views.Settings;
using Pixeval.Views.Viewers;
using TabView.Avalonia;

namespace Pixeval.Views.ViewContainers;

public partial class TabViewContainer : ViewContainerBase
{
    public static readonly DirectProperty<TabViewContainer, bool> CanCreateNewTabProperty =
        AvaloniaProperty.RegisterDirect<TabViewContainer, bool>(
            nameof(CanCreateNewTab),
            static container => container.CanCreateNewTab);

    private NavigationConfiguration _navigationConfiguration = null!;

    public ObservableCollection<NavigationMenuItem> HeaderNavigationItems { get; } = [];

    public ObservableCollection<NavigationMenuItem> FooterNavigationItems { get; } = [];

    public static ICommand OpenNavigationItemCommand { get; } = new RelayCommand<Control>(OpenNavigationItem);

    public bool CanCreateNewTab
    {
        get;
        private set => SetAndRaise(CanCreateNewTabProperty, ref field, value);
    }

    static TabViewContainer()
    {
        RegisterNavigationItemInputHandlers<Button>();
        RegisterNavigationItemInputHandlers<MenuItem>();
        ContextRequestedEvent.AddClassHandler<TabsViewItem>(
            TabsViewItem_OnContextRequested,
            RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    public TabViewContainer()
    {
        InitializeComponent();

        RebuildNavigation();

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
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Manager = new WindowNotificationManager(TopLevel.GetTopLevel(this))
        {
            MaxItems = 3,
            Position = NotificationPosition.BottomRight
        };
        RegisterContentDialogHost(TopLevel.GetTopLevel(this));

        await AppInfo.AppVersion.GitHubCheckForUpdateAsync();
        var dialogTasks = new List<Task<ContentDialogResult>>();
        if (App.AppViewModel.AppSettings.IsNewVersion)
            dialogTasks.Add(CreateAcknowledgementAsync(
                SettingsPage.ReleaseTitle,
                SettingsPage.CreateReleaseNotes(AppInfo.AppVersion.CurrentAppReleaseModel)));
        if (AppInfo.AppVersion is { UpdateAvailable: true, NewestAppReleaseModel: { } release })
            dialogTasks.Add(CreateAcknowledgementAsync(
                SettingsPage.GetReleaseTitle(release.Version),
                SettingsPage.CreateReleaseNotes(release)));

        await Task.WhenAll(dialogTasks);
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
            {
                DisposeTab(selected);
                _ = pages.Remove(selected);
            }
    }

    public void ReloadNavigation() => RebuildNavigation();

    private void RebuildNavigation()
    {
        _navigationConfiguration = NavigationYamlParser.ParseOrDefault(App.AppViewModel.NavigationMenuYamlText);
        HeaderNavigationItems.Clear();
        FooterNavigationItems.Clear();
        foreach (var item in _navigationConfiguration.HeaderItems)
            HeaderNavigationItems.Add(item);
        foreach (var item in _navigationConfiguration.FooterItems)
            FooterNavigationItems.Add(item);
        CanCreateNewTab = _navigationConfiguration.NewTabPage is not null;
    }

    private static void OpenNavigationItem(Control? control) => _ = TryOpenNavigationItem(control, openNew: false);

    private static bool TryOpenNavigationItem(Control? control, bool openNew)
    {
        if (control is not { DataContext: NavigationPageItem { PageType: { } type } }
            || TopLevel.GetTopLevel(control) is not { ViewContainer: { } viewContainer })
            return false;

        if (!openNew
            && viewContainer is TabViewContainer tabViewContainer
            && tabViewContainer.TrySelectPage(type))
            return true;

        viewContainer.NavigateTo((Page) Activator.CreateInstance(type)!);
        return true;
    }

    private bool TrySelectPage(Type pageType)
    {
        if (TabsControl.Pages is not IList<Page> pages)
            return false;

        if (TabsControl.SelectedPage?.GetType() == pageType)
            return true;

        for (var index = pages.Count - 1; index >= 0; --index)
        {
            if (pages[index].GetType() != pageType)
                continue;

            TabsControl.SelectedIndex = index;
            return true;
        }

        return false;
    }

    private static void RegisterNavigationItemInputHandlers<T>() where T : Control
    {
        PointerReleasedEvent.AddClassHandler<T>(NavigationItem_OnPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        ContextRequestedEvent.AddClassHandler<T>(NavigationItem_OnContextRequested, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private static void NavigationItem_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton is MouseButton.Middle
            && sender is Control control
            && IsNavigationItemControl(control)
            && TryOpenNavigationItem(control, openNew: true))
            e.Handled = true;
    }

    private static void NavigationItem_OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (sender is Control control
            && IsNavigationItemControl(control)
            && TryOpenNavigationItem(control, openNew: true))
            e.Handled = true;
    }

    private static bool IsNavigationItemControl(Control control) => control switch
    {
        Button { Command: { } command } => ReferenceEquals(command, OpenNavigationItemCommand),
        MenuItem { Command: { } command } => ReferenceEquals(command, OpenNavigationItemCommand),
        _ => false
    };

    private static void TabsViewItem_OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (sender is not TabsViewItem tabItem
            || (tabItem.DataContext as Page ?? tabItem.Content as Page) is not { } contextPage
            || TopLevel.GetTopLevel(tabItem)?.ViewContainer is not TabViewContainer container
            || container.TabsControl.Pages is not IList<Page> pages
            || !pages.Contains(contextPage))
            return;

        var snapshot = pages.ToArray();
        var menu = new ContextMenu
        {
            ItemsSource = new MenuItem[]
            {
                CreateTabCloseMenuItem(container, contextPage, snapshot, TabCloseScope.Others),
                CreateTabCloseMenuItem(container, contextPage, snapshot, TabCloseScope.Left),
                CreateTabCloseMenuItem(container, contextPage, snapshot, TabCloseScope.Right)
            }
        };
        tabItem.ContextMenu = menu;
        menu.Open(tabItem);
        e.Handled = true;
    }

    private static MenuItem CreateTabCloseMenuItem(
        TabViewContainer container,
        Page contextPage,
        IReadOnlyList<Page> pages,
        TabCloseScope scope)
    {
        var item = new MenuItem
        {
            Header = I18NManager.GetResource(scope switch
            {
                TabCloseScope.Others => MainPageResources.TabContextMenuCloseOtherTabs,
                TabCloseScope.Left => MainPageResources.TabContextMenuCloseTabsToLeft,
                TabCloseScope.Right => MainPageResources.TabContextMenuCloseTabsToRight,
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
            }),
            IsEnabled = TabClosePlanner.GetTargets(pages, contextPage, scope).Count is not 0
        };
        item.Click += (_, _) => container.CloseTabs(contextPage, scope);
        return item;
    }

    private void CloseTabs(Page contextPage, TabCloseScope scope)
    {
        if (TabsControl.Pages is not IList<Page> pages || !pages.Contains(contextPage))
            return;

        TabsControl.SelectedIndex = pages.IndexOf(contextPage);
        // 先固定关闭目标，避免移除页面时索引变化导致关闭范围漂移。
        var targets = TabClosePlanner.GetTargets(pages.ToArray(), contextPage, scope);
        foreach (var target in targets)
        {
            // 复用单标签关闭的释放事件，让页面有机会处理自己的 ViewModel 生命周期。
            var args = new RoutedEventArgs(ViewModelDisposal.RequestDisposeEvent, target);
            target.RaiseEvent(args);
            if (!args.Handled)
                DisposeTab(target);
            _ = pages.Remove(target);
        }
    }

    private void TabsView_OnAddTabButtonClick(TabsView sender, EventArgs e)
    {
        if (_navigationConfiguration.NewTabPage is { PageType: { } type })
            NavigateTo((Page) Activator.CreateInstance(type)!);
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
        if (e.Source is not Control source || TabsControl.Pages is not IReadOnlyCollection<Page> pages)
            return;

        var page = source is Page sourcePage && pages.Contains(sourcePage)
            ? sourcePage
            : source.GetLogicalAncestors().OfType<Page>().FirstOrDefault(pages.Contains);
        if (page is null)
            return;

        ViewModelDisposal.Register(page, e.Disposable);
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
        ViewModelDisposal.Dispose(tabItem);
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
                {
                    foreach (var page in container.TabsControl.Pages ?? [])
                        DisposeTab(page);
                    if (container.DataContext is IDisposable disposable)
                        disposable.Dispose();
                    container.DataContext = null;
                }
            };

            return new TabsHost(window, container.TabsControl);
        }
    }
}
