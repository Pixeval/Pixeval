using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace FluentAvalonia.UI.Navigation;

/// <summary>
/// Represents an entry in the BackStack or ForwardStack of a Frame.
/// </summary>
public class PageStackEntry
{
    /// <summary>
    /// Initializes a new instance of the PageStackEntry class.
    /// </summary>
    /// <param name="sourcePageType">The type of page associated with the navigation entry, as a type reference</param>
    /// <param name="parameter">The navigation parameter associated with the navigation entry.</param>
    /// <param name="navigationTransitionInfo">Info about the animated transition associated with the navigation entry.</param>
    public PageStackEntry([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type sourcePageType, object? parameter, NavigationTransitionInfo? navigationTransitionInfo)
    {
        NavigationTransitionInfo = navigationTransitionInfo;
        SourcePageType = sourcePageType;
        Parameter = parameter;
    }

    /// <summary>
    /// Gets the type of page associated with this navigation entry.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type SourcePageType { get; set; }

    /// <summary>
    /// Gets a value that indicates the animated transition associated with the navigation entry.
    /// </summary>
    public NavigationTransitionInfo? NavigationTransitionInfo { get; internal set; }

    /// <summary>
    /// Gets the navigation parameter associated with this navigation entry.
    /// </summary>
    public object? Parameter { get; set; }

    /// <summary>
    /// Gets the navigation context used for this page when called from 
    /// <see cref="FluentAvalonia.UI.Controls.Frame.NavigateFromObject"/>
    /// </summary>
    public object? Context { get; internal set; }

    internal Control? Instance { get; set; }
}
