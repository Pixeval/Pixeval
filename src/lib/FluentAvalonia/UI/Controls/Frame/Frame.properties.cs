using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Navigation;

namespace FluentAvalonia.UI.Controls;

public partial class Frame : ContentControl
{
    /// <summary>
    /// Defines the <see cref="SourcePageType"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, Type> SourcePageTypeProperty =
        AvaloniaProperty.RegisterDirect<Frame, Type>(nameof(SourcePageType),
            frame => frame.SourcePageType);

    /// <summary>
    /// Defines the <see cref="CacheSize"/> property
    /// </summary>
    public static readonly StyledProperty<int> CacheSizeProperty =
        AvaloniaProperty.Register<Frame, int>(nameof(CacheSize),
            defaultValue: 10, 
            coerce: (x, v) => v >= 0 ? v : 0);

    /// <summary>
    /// Defines the <see cref="BackStackDepth"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, int> BackStackDepthProperty =
        AvaloniaProperty.RegisterDirect<Frame, int>(nameof(BackStackDepth),
            x => x.BackStackDepth);

    /// <summary>
    /// Defines the <see cref="CanGoBack"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, bool> CanGoBackProperty =
        AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(CanGoBack),
            x => x.CanGoBack);

    /// <summary>
    /// Defines the <see cref="CanGoForward"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, bool> CanGoForwardProperty =
        AvaloniaProperty.RegisterDirect<Frame, bool>(nameof(CanGoForward),
            x => x.CanGoForward);

    /// <summary>
    /// Defines the <see cref="CurrentSourcePageType"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, Type?> CurrentSourcePageTypeProperty =
        AvaloniaProperty.RegisterDirect<Frame, Type?>(nameof(CurrentSourcePageType),
            x => x.CurrentSourcePageType);

    /// <summary>
    /// Defines the <see cref="BackStack"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, IList<PageStackEntry>> BackStackProperty =
        AvaloniaProperty.RegisterDirect<Frame, IList<PageStackEntry>>(nameof(BackStack),
            x => x.BackStack);

    /// <summary>
    /// Defines the <see cref="ForwardStack"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, IList<PageStackEntry>> ForwardStackProperty =
        AvaloniaProperty.RegisterDirect<Frame, IList<PageStackEntry>>(nameof(ForwardStack),
            x => x.ForwardStack);

    /// <summary>
    /// Defines the <see cref="IsNavigationStackEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsNavigationStackEnabledProperty =
        AvaloniaProperty.Register<Frame, bool>(nameof(IsNavigationStackEnabled),
            defaultValue: true);

    /// <summary>
    /// Defines the <see cref="NavigationPageFactory"/> property
    /// </summary>
    public static readonly DirectProperty<Frame, INavigationPageFactory?> NavigationPageFactoryProperty =
        AvaloniaProperty.RegisterDirect<Frame, INavigationPageFactory?>(nameof(NavigationPageFactory),
            x => x.NavigationPageFactory, (x, v) => x.NavigationPageFactory = v);

    /// <summary>
    /// Gets or sets a type reference of the current content, or the content that should be navigated to.
    /// </summary>
    public Type SourcePageType
    {
        get;
        private set => SetAndRaise(SourcePageTypeProperty, ref field, value);
    } = null!;

    /// <summary>
    /// Gets or sets the number of pages in the navigation history that can be cached for the frame.
    /// </summary>
    public int CacheSize
    {
        get => GetValue(CacheSizeProperty);
        set => SetValue(CacheSizeProperty, value);
    }

    /// <summary>
    /// Gets the number of entries in the navigation back stack.
    /// </summary>
    public int BackStackDepth => _backStack.Count;

    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in back navigation history.
    /// </summary>
    public bool CanGoBack => _backStack.Count > 0;

    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in forward navigation history.
    /// </summary>
    public bool CanGoForward => _forwardStack.Count > 0;

    /// <summary>
    /// Gets a type reference for the content that is currently displayed.
    /// </summary>
    public Type? CurrentSourcePageType => Content?.GetType();

    /// <summary>
    /// Gets a collection of <see cref="PageStackEntry"/> instances representing the 
    /// backward navigation history of the Frame.
    /// </summary>
    public IList<PageStackEntry> BackStack
    {
        get => _backStack;
        private set => SetAndRaise(BackStackProperty, ref _backStack, value);
    }

    /// <summary>
    /// Gets a collection of <see cref="PageStackEntry"/> instances representing the 
    /// forward navigation history of the Frame.
    /// </summary>
    public IList<PageStackEntry> ForwardStack
    {
        get => _forwardStack;
        private set => SetAndRaise(ForwardStackProperty, ref _forwardStack, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether navigation is recorded in the Frame's 
    /// <see cref="ForwardStack"/> or <see cref="BackStack"/>.
    /// </summary>
    public bool IsNavigationStackEnabled
    {
        get => GetValue(IsNavigationStackEnabledProperty); 
        set => SetValue(IsNavigationStackEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the user specified factory that should be used for resolving pages
    /// when types are not controls or from object instances directly
    /// </summary>
    public INavigationPageFactory? NavigationPageFactory
    {
        get;
        set => SetAndRaise(NavigationPageFactoryProperty, ref field, value);
    }

    internal PageStackEntry? CurrentEntry { get; set; }

    /// <summary>
    /// Occurs when the content that is being navigated to has been found and is available 
    /// from the Content property, although it may not have completed loading.
    /// </summary>
    public event NavigatedEventHandler? Navigated;

    /// <summary>
    /// Occurs when a new navigation is requested.
    /// </summary>
    public event NavigatingCancelEventHandler? Navigating;

    /// <summary>
    /// Occurs when an error is raised while navigating to the requested content.
    /// </summary>
    public event NavigationFailedEventHandler? NavigationFailed;

    /// <summary>
    /// Occurs when a new navigation is requested while a current navigation is in progress.
    /// </summary>
    public event NavigationStoppedEventHandler? NavigationStopped;

    /// <summary>
    /// Indicates to a page that it is being navigated away from. Takes the place of 
    /// Microsoft.UI.Xaml.Controls.Page.OnNavigatingFrom() method
    /// </summary>
    public static readonly RoutedEvent<NavigatingCancelEventArgs> NavigatingFromEvent =
        RoutedEvent.Register<Control, NavigatingCancelEventArgs>("NavigatingFrom", RoutingStrategies.Direct);

    /// <summary>
    /// Indicates to a page that it has been navigated away from. Takes the place of
    /// Microsoft.UI.Xaml.Controls.Page.OnNavigatedFrom() method
    /// </summary>
    public static readonly RoutedEvent<NavigationEventArgs> NavigatedFromEvent =
        RoutedEvent.Register<Control, NavigationEventArgs>("NavigatedFrom", RoutingStrategies.Direct);

    /// <summary>
    /// Indicates to a page that it is being navigated to. Takes the place of
    /// Microsoft.UI.Xaml.Controls.Page.OnNavigatedTo() method
    /// </summary>
    public static readonly RoutedEvent<NavigationEventArgs> NavigatedToEvent =
        RoutedEvent.Register<Control, NavigationEventArgs>("NavigatedTo", RoutingStrategies.Direct);

    private IList<PageStackEntry> _backStack;
    private IList<PageStackEntry> _forwardStack;
}
