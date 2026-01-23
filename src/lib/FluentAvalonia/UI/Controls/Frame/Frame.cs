using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Displays <see cref="UserControl"/> instances (Pages in WinUI), supports navigation to new pages, 
/// and maintains a navigation history to support forward and backward navigation.
/// </summary>
[TemplatePart(TpContentPresenter, typeof(ContentPresenter))]
public partial class Frame : ContentControl
{
    public Frame()
    {
        var back = new AvaloniaList<PageStackEntry>();
        var forw = new AvaloniaList<PageStackEntry>();

        back.CollectionChanged += OnBackStackChanged;
        forw.CollectionChanged += OnForwardStackChanged;

        _backStack = back;
        _forwardStack = forw;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            if (change.NewValue is null)
            {
                CurrentEntry = null;
            }
        }
        else if (change.Property == IsNavigationStackEnabledProperty)
        {
            if (!change.GetNewValue<bool>())
            {
                BackStack.Clear();
                ForwardStack.Clear();
                _pageCache.Clear();
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _presenter = e.NameScope.Find<ContentPresenter>(TpContentPresenter);
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        return presenter.Name is TpContentPresenter || base.RegisterContentPresenter(presenter);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (e.Root is TopLevel tl)
        {
            tl.BackRequested += OnTopLevelBackRequested;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (e.Root is TopLevel tl)
        {
            tl.BackRequested -= OnTopLevelBackRequested;
        }
    }

    /// <summary>
    /// Navigates to the most recent item in back navigation history, if a Frame manages its own navigation history.
    /// </summary>
    public void GoBack() => GoBack(null);

    /// <summary>
    /// Navigates to the most recent item in back navigation history, if a Frame manages its own navigation history, 
    /// and specifies the animated transition to use.
    /// </summary>
    /// <param name="infoOverride">Info about the animated transition to use.</param>
    public void GoBack(NavigationTransitionInfo? infoOverride)
    {
        if (!CanGoBack)
            return;
        var entry = _backStack[^1];
        entry.NavigationTransitionInfo = infoOverride ?? CurrentEntry?.NavigationTransitionInfo;

        NavigateCore<DiscardControl>(entry, NavigationMode.Back);
    }

    /// <summary>
    /// Navigates to the most recent item in forward navigation history, if a Frame manages its own navigation history.
    /// </summary>
    public void GoForward()
    {
        if (CanGoForward)
        {
            NavigateCore<DiscardControl>(_forwardStack[^1], NavigationMode.Forward);
        }
    }

    /// <summary>
    /// Causes the Frame to load content represented by the specified Page -derived data type, 
    /// also passing a parameter to be interpreted by the target of the navigation, and a value 
    /// indicating the animated transition to use.
    /// </summary>
    /// <typeparam name="T">The page (IControl) to navigate to, specified as a type reference to its class type, or 
    /// if a <see cref="NavigationPageFactory"/> this can be any type (e.g., a ViewModel)</typeparam>
    /// <param name="parameter">The navigation parameter to pass to the target page.</param>
    /// <param name="infoOverride">Info about the animated transition.</param>
    /// <returns><c>false</c> if a <see cref="NavigationFailed"/> event handler has set Handled to true; 
    /// otherwise, <c>true</c>.</returns>
    public bool Navigate<T>(object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        where T : Control, new()
    {
        return NavigateCore<DiscardControl>(new PageStackEntry(typeof(T), parameter,
            infoOverride), NavigationMode.New);
    }

    /// <summary>
    /// Causes the Frame to load content represented by the specified Page -derived data type, 
    /// also passing a parameter to be interpreted by the target of the navigation, and a value 
    /// indicating the animated transition to use.
    /// </summary>
    /// <param name="sourcePageType">The page (IControl) to navigate to, specified as a type reference to its class type, or 
    /// if a <see cref="NavigationPageFactory"/> this can be any type (e.g., a ViewModel)</param>
    /// <param name="parameter">The navigation parameter to pass to the target page.</param>
    /// <param name="infoOverride">Info about the animated transition.</param>
    /// <returns><c>false</c> if a <see cref="NavigationFailed"/> event handler has set Handled to true; 
    /// otherwise, <c>true</c>.</returns>
    public bool Navigate([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type sourcePageType, object? parameter = null, NavigationTransitionInfo? infoOverride = null)
    {
        return NavigateCore<DiscardControl>(new PageStackEntry(sourcePageType, parameter,
            infoOverride), NavigationMode.New);
    }

    /// <summary>
    /// Causes the Frame to load content represented by the specified Page, also passing a parameter to be 
    /// interpreted by the target of the navigation.
    /// </summary>
    /// <param name="sourcePageType">The page (IControl) to navigate to, specified as a type reference to its class type, or 
    /// if a <see cref="NavigationPageFactory"/> this can be any type (e.g., a ViewModel)</param>
    /// <param name="parameter">The navigation parameter to pass to the target page.</param>
    /// <param name="navOptions">Options for the navigation, including whether it is recorded in the navigation stack 
    /// and what transition animation is used.</param>
    /// <returns><see langword="false"/> if a <see cref="NavigationFailed"/> event handler has set Handled to true; 
    /// otherwise, <see langword="true"/>.</returns>
    public bool NavigateToType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type sourcePageType, object? parameter, FrameNavigationOptions? navOptions) =>
        NavigateCore<DiscardControl>(new PageStackEntry(sourcePageType, parameter, navOptions?.TransitionInfoOverride),
            NavigationMode.New, navOptions);

    /// <summary>
    /// Causes the frame to load content represented by the specified target property with the
    /// specified navigation options
    /// </summary>
    /// <remarks>
    /// You must specify a <see cref="NavigationPageFactory"/> for this method to succeed
    /// </remarks>
    /// <param name="target">An existing object for which page creation should be based (e.g., A ViewModel instance)</param>
    /// <param name="parameter">The navigation parameter to pass to the target page.</param>
    /// <param name="navOptions">Options for the navigation, including whether it is recorded in the navigation stack 
    /// and what transition animation is used.</param>
    /// <returns><c>false</c> if a <see cref="NavigationFailed"/> event handler has set Handled to true or
    /// if <see cref="NavigationPageFactory" /> is not specified; otherwise, <see langword="true"/>.</returns>
    public bool NavigateFromObject(object target, object? parameter = null, FrameNavigationOptions? navOptions = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        // Check the cache first to see if we have an existing page that matches
        // For this check we check by both type and object reference
        var existing = CheckCacheAndGetPage(null, target);

        if (existing is null)
        {
            ArgumentNullException.ThrowIfNull(NavigationPageFactory);
            // If we don't have a previous reference, try to resolve via Factory
            existing = NavigationPageFactory.GetPageFromObject(target);

            // Unable to locate page, return false
            if (existing is null)
                return false;
        }

        // The page source Type here will be whatever was specified as 'target'
        var entry = new PageStackEntry(typeof(Control), parameter, navOptions?.TransitionInfoOverride)
        {
            Instance = existing,
            Context = target
        };

        return NavigateCore<DiscardControl>(entry, NavigationMode.New, navOptions);
    }

    private bool NavigateCore<T>(PageStackEntry entry, NavigationMode mode, FrameNavigationOptions? options = null)
        where T : Control, new()
    {
        try
        {
            _isNavigating = true;

            var ea = new NavigatingCancelEventArgs(mode,
                entry.NavigationTransitionInfo,
                entry.Parameter,
                entry.SourcePageType);

            Navigating?.Invoke(this, ea);

            if (ea.Cancel)
            {
                OnNavigationStopped(entry, mode);
                return false;
            }

            // Tell the current page we want to navigate away from it
            if (CurrentEntry?.Instance is { } oldPage)
            {
                ea.RoutedEvent = NavigatingFromEvent;
                oldPage.RaiseEvent(ea);

                if (ea.Cancel)
                {
                    OnNavigationStopped(entry, mode);
                    return false;
                }
            }

            // Navigate to new page
            var prevEntry = CurrentEntry;
            var wasPageSet = entry.Instance is not null;

            if (mode is NavigationMode.New && !wasPageSet)
            {
                // Check if we already have an instance of the page in the cache
                entry.Instance = CheckCacheAndGetPage(entry.SourcePageType);
            }

            if (entry.Instance is null)
            {
                var page = CreatePageAndCacheIfNecessary<T>(entry.SourcePageType);
                if (page is null)
                {
                    throw new ArgumentException($"The type {entry.SourcePageType} is not a valid page type.");
                }

                entry.Instance = page;
            }
            else if (wasPageSet)
            {
                // The page was already create for us when passed in (NavigateFromObject path)
                // Try adding to the cache now
                TryAddToCache(entry.Context!, entry.Instance);
            }

            var oldEntry = CurrentEntry;
            CurrentEntry = entry;

            var navEa = new NavigationEventArgs(
                CurrentEntry.Instance,
                mode,
                entry.NavigationTransitionInfo,
                entry.Parameter,
                entry.SourcePageType);

            // Old page is now unloaded, raise OnNavigatedFrom
            if (oldEntry != null)
            {
                navEa.RoutedEvent = NavigatedFromEvent;
                oldEntry.Instance!.RaiseEvent(navEa);
            }

            SetContentAndAnimate(entry);

            var addToNavStack = options?.IsNavigationStackEnabled ?? IsNavigationStackEnabled;

            if (addToNavStack)
            {
                switch (mode)
                {
                    case NavigationMode.New:
                        ForwardStack.Clear();
                        if (prevEntry is not null)
                        {
                            if (BackStack.Count == CacheSize)
                                if (BackStack.Count > 0)
                                    BackStack.RemoveAt(0);

                            BackStack.Add(prevEntry);
                        }
                        break;

                    case NavigationMode.Back:
                        ArgumentNullException.ThrowIfNull(prevEntry);
                        ForwardStack.Add(prevEntry);
                        BackStack.Remove(CurrentEntry);
                        break;

                    case NavigationMode.Forward:
                        ArgumentNullException.ThrowIfNull(prevEntry);
                        BackStack.Add(prevEntry);
                        ForwardStack.Remove(CurrentEntry);
                        break;

                    case NavigationMode.Refresh:
                        break;
                }
            }


            SourcePageType = entry.SourcePageType;
            //CurrentSourcePageType = entry.SourcePageType;

            Navigated?.Invoke(this, navEa);

            // New Page is loaded, let's tell the page
            // Now posted to dispatcher to ensure page has loaded - enabling composition
            // animations to work now - CompositionVisuals *should* be ready now
            Dispatcher.UIThread.Post(() =>
            {
                if (entry.Instance is { } newPage)
                {
                    navEa.RoutedEvent = NavigatedToEvent;
                    newPage.RaiseEvent(navEa);
                }
            }, DispatcherPriority.Render);

            //Need to find compatible method for this
            //VisualTreeHelper.CloseAllPopups();

            return true;
        }
        catch (Exception ex)
        {
            NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(ex, entry.SourcePageType));
            return false;
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private void OnNavigationStopped(PageStackEntry entry, NavigationMode mode)
    {
        NavigationStopped?.Invoke(this, new NavigationEventArgs(entry.Instance,
            mode, entry.NavigationTransitionInfo, entry.Parameter, entry.SourcePageType));
    }

    private void OnForwardStackChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var oldCount = _forwardStack.Count - (e.NewItems?.Count ?? 0) + (e.OldItems?.Count ?? 0);

        var oldForward = oldCount > 0;
        var newForward = _forwardStack.Count > 0;
        RaisePropertyChanged(CanGoForwardProperty, oldForward, newForward);
    }

    private void OnBackStackChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var oldCount = (_backStack.Count - (e.NewItems?.Count ?? 0) + (e.OldItems?.Count ?? 0));

        var oldBack = oldCount > 0;
        var newBack = _backStack.Count > 0;
        RaisePropertyChanged(CanGoBackProperty, oldBack, newBack);
        RaisePropertyChanged(BackStackDepthProperty, oldCount, _backStack.Count);
    }

    private Control? CreatePageAndCacheIfNecessary<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type srcPageType)
        where T : Control, new()
    {
        if (CacheSize is 0)
        {
            return NavigationPageFactory?.GetPage(srcPageType) ?? Create<T>(srcPageType);
        }

        // This is triggered via Navigate(Type) - we only need to check the page type here
        if (_pageCache.Any(t => t.PageSrcType == srcPageType))
        {
            throw new Exception($"An object of type {srcPageType} has already been added to the Navigation Stack");
        }

        var newPage = NavigationPageFactory?.GetPage(srcPageType) ?? Create<T>(srcPageType);

        if (newPage is not null)
        {
            _pageCache.Add(new NavigationCacheItem(srcPageType, null, newPage));

            if (_pageCache.Count > CacheSize)
            {
                _pageCache.RemoveAt(0);
            }
        }

        return newPage;
    }

    private static Control? Create<T>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type srcPageType)
        where T : Control, new()
    {
        if (typeof(T) == typeof(DiscardControl) && srcPageType != typeof(DiscardControl))
            return Activator.CreateInstance(srcPageType) as Control;

        return new T();
    }

    private Control? CheckCacheAndGetPage(Type? srcPageType = null, object? target = null)
    {
        if (CacheSize == 0)
            return null;

        // v2 - Changes for cache
        // A page cached by Navigate(Type) and NavigateFromObject will be cached 
        // separately and not shared between the two - users should be consistent
        // here. Thus, srcType should be null when context isn't and vice versa
        foreach (var item in _pageCache.AsEnumerable().Reverse())
        {
            if (srcPageType is not null && item.PageSrcType == srcPageType)
            {
                // Call to Navigate(Type)
                return item.Page;
            }
            if (target is not null && item.Context == target)
            {
                // Call to NavigateFromObject()
                return item.Page;
            }
        }

        return null;
    }

    private void TryAddToCache(object context, Control page)
    {
        // This is trigger by NavigateFromObject()

        // v2 - Changes for cache
        // A page cached by Navigate(Type) and NavigateFromObject will be cached 
        // separately and not shared between the two - users should be consistent
        // here. Thus, srcType should be null when context isn't and vice versa
        foreach (var item in _pageCache.AsEnumerable().Reverse())
        {
            if (context != null && item.Context == context)
            {
                // Call to NavigateFromObject() - page is already cached
                return;
            }
        }

        // Page is not cached - add it
        _pageCache.Add(new NavigationCacheItem(null, context, page));

        if (_pageCache.Count > CacheSize)
        {
            _pageCache.RemoveAt(0);
        }
    }

    private void SetContentAndAnimate(PageStackEntry entry)
    {
        // Guard
        if (entry == null!)
            return;

        Content = entry.Instance;

        if (_presenter != null)
        {
            // Default to entrance transition
            entry.NavigationTransitionInfo ??= new EntranceNavigationTransitionInfo();
            _presenter.Opacity = 0;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            // Post the animation otherwise pages that take slightly longer to load won't
            // have an animation since it will run before layout is complete
            Dispatcher.UIThread.Post(() =>
            {
                entry.NavigationTransitionInfo.RunAnimation(_presenter, _cts.Token);
            }, DispatcherPriority.Render);
        }
    }

    private void OnTopLevelBackRequested(object? sender, RoutedEventArgs e)
    {
        if (!e.Handled && IsNavigationStackEnabled && CanGoBack)
        {
            GoBack();
            e.Handled = true;
        }
    }

    private CancellationTokenSource? _cts;
    private ContentPresenter? _presenter;
    //private readonly List<(Type pageSrcType, Control page)> _cache = new List<(Type, Control)>(10);
    private readonly List<NavigationCacheItem> _pageCache = new(10);
    private bool _isNavigating;

    private const string TpContentPresenter = "ContentPresenter";

    private class NavigationCacheItem
    {
        public NavigationCacheItem(Type? pageType, object? context, Control page)
        {
            if (pageType != null && context != null)
                throw new InvalidOperationException("PageType and Context cannot both be set");

            PageSrcType = pageType;
            Context = context;
            Page = page;
        }

        public readonly Type? PageSrcType;
        public readonly object? Context;
        public readonly Control Page;
    }
}

/// <summary>
/// Use <see cref="Activator.CreateInstance(Type)"/> when with <see cref="DiscardControl"/>
/// </summary>
file class DiscardControl : Control;
