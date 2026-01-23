using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Media.Animation;

namespace FluentAvalonia.UI.Navigation;

/// <summary>
/// Represents the method that will handle the Navigated event.
/// </summary>
/// <param name="sender">The object where the handler is attached.</param>
/// <param name="e">Event data for the event.</param>
public delegate void NavigatedEventHandler(object sender, NavigationEventArgs e);

/// <summary>
/// Provides event data for the NavigationStopped event.
/// </summary>
/// <param name="sender">The object where the handler is attached.</param>
/// <param name="e">Event data for the event.</param>
public delegate void NavigationStoppedEventHandler(object sender, NavigationEventArgs e);

/// <summary>
/// Provides data for navigation methods and event handlers that cannot cancel the navigation request.
/// </summary>
public class NavigationEventArgs : RoutedEventArgs
{
    internal NavigationEventArgs(Control? content, NavigationMode mode,
        NavigationTransitionInfo? navInfo, object? param,
        Type srcPgType)
    {
        Content = content;
        NavigationMode = mode;
        NavigationTransitionInfo = navInfo;
        Parameter = param;
        SourcePageType = srcPgType;
    }

    /// <summary>
    /// Gets the root node of the target page's content.
    /// </summary>
    public Control? Content { get; }

    /// <summary>
    /// Gets a value that indicates the direction of movement during navigation
    /// </summary>
    public NavigationMode NavigationMode { get; }

    /// <summary>
    /// Gets any "Parameter" object passed to the target page for the navigation.
    /// </summary>
    public object? Parameter { get; }

    /// <summary>
    /// Gets the data type of the source page.
    /// </summary>
    public Type SourcePageType { get; }

    /// <summary>
    /// Gets a value that indicates the animated transition associated with the navigation.
    /// </summary>
    public NavigationTransitionInfo? NavigationTransitionInfo { get; }

    public T GetParameter<T>() => (T)Parameter!;
}
