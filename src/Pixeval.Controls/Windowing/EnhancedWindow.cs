// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.Windowing;

[WindowSizeHelper]
public sealed partial class EnhancedWindow : Window
{
    public ulong HWnd => AppWindow.Id.Value;

    private readonly EnhancedWindow? _owner;

    public EnhancedPage PageContent
    {
        get => Content.To<Grid>().Children[0].To<EnhancedPage>();
        set => Content.To<Grid>().Children[0] = value;
    }

    public bool IsMaximize => AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Maximized };

    internal EnhancedWindow(EnhancedPage content)
    {
        content.HorizontalAlignment = HorizontalAlignment.Stretch;
        content.VerticalAlignment = VerticalAlignment.Stretch;
        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        Growl.SetGrowlParent(stackPanel, true);
        Growl.SetToken(stackPanel, HWnd);
        Content = new Grid
        {
            Children =
            {
                content,
                stackPanel
            }
        };
        Closed += OnClosed;
    }

    internal EnhancedWindow(EnhancedWindow owner, EnhancedPage content) : this(content)
    {
        _owner = owner;
        _owner.AppWindow.Closing += OnOwnerOnClosing;
    }

    public event RoutedEventHandler Initialized
    {
        add => PageContent.Loaded += value;
        remove => PageContent.Loaded -= value;
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        if (_owner is not null)
            _owner.AppWindow.Closing -= OnOwnerOnClosing;
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void OnOwnerOnClosing(AppWindow sender, AppWindowClosingEventArgs e) => Close();
}
