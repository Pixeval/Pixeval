using System;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Media.Animation;

namespace FluentAvalonia.UI.Navigation;

/// <summary>
/// Represents the method to use as the OnNavigatingFrom callback override.
/// </summary>
/// <param name="sender">The object where the method is implemented.</param>
/// <param name="e">Event data that is passed through the callback.</param>
public delegate void NavigatingCancelEventHandler(object sender, NavigatingCancelEventArgs e);

/// <summary>
/// Provides data for the OnNavigatingFrom callback that can be used to cancel a navigation request from origination.
/// </summary>
public class NavigatingCancelEventArgs : RoutedEventArgs
{
    internal NavigatingCancelEventArgs(NavigationMode mode, NavigationTransitionInfo? info,
        object? param, Type srcType)
    {
        NavigationMode = mode;
        NavigationTransitionInfo = info;
        Parameter = param;
        SourcePageType = srcType;
    }

    /// <summary>
    /// Specifies whether a pending navigation should be canceled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the value of the mode parameter from the originating Navigate call.
    /// </summary>
    public NavigationMode NavigationMode { get; }

    /// <summary>
    /// Gets the value of the SourcePageType parameter from the originating Navigate call.
    /// </summary>
    public Type SourcePageType { get; }

    /// <summary>
    /// Gets a value that indicates the animated transition associated with the navigation.
    /// </summary>
    public NavigationTransitionInfo? NavigationTransitionInfo { get; }

    /// <summary>
    /// Gets the navigation parameter associated with this navigation.
    /// </summary>
    public object? Parameter { get; }
}
