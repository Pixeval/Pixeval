// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Reflection;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace Pixeval.Util.Triggers;

/// <summary>
/// <seealso href="https://github.com/microsoft/XamlBehaviors/blob/master/src/BehaviorsSDKManaged/Microsoft.Xaml.Interactions.Shared/Core/EventTriggerBehavior.cs"/>
/// </summary>
public partial class EventWithConditionTriggerBehavior : Trigger
{
    [GeneratedDependencyProperty(DefaultValue = "Loaded")]
    public partial string EventName { get; set; }

    [GeneratedDependencyProperty]
    public partial object? SourceObject { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool IsActive { get; set; }

    private Delegate? _eventHandler;
    private bool _isLoadedEventRegistered;
    private object? _resolvedSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventWithConditionTriggerBehavior"/> class.
    /// </summary>
    public EventWithConditionTriggerBehavior()
    {
    }

    /// <summary>
    /// Called after the behavior is attached to the <see cref="Behavior.AssociatedObject"/>.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        SetResolvedSource(ComputeResolvedSource());
    }

    /// <summary>
    /// Called when the behavior is being detached from its <see cref="Behavior.AssociatedObject"/>.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        SetResolvedSource(null);
    }

    private void SetResolvedSource(object? newSource)
    {
        if (AssociatedObject is null || _resolvedSource == newSource)
        {
            return;
        }

        if (_resolvedSource is not null)
        {
            UnregisterEvent(EventName);
        }

        _resolvedSource = newSource;

        if (_resolvedSource is not null)
        {
            RegisterEvent(EventName);
        }
    }

    private object? ComputeResolvedSource()
    {
        // If the SourceObject property is set at all, we want to use it. It is possible that it is data
        // bound and bindings haven't been evaluated yet. Plus, this makes the API more predictable.
        return ReadLocalValue(SourceObjectProperty) != DependencyProperty.UnsetValue ? SourceObject : AssociatedObject;
    }

    private void RegisterEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            return;
        }

        if (eventName != "Loaded")
        {
            var sourceObjectType = _resolvedSource?.GetType();
            if (sourceObjectType?.GetRuntimeEvent(eventName) is not { } info)
                return;

            var methodInfo = typeof(EventWithConditionTriggerBehavior).GetTypeInfo().GetDeclaredMethod("OnEvent");
            _eventHandler = methodInfo!.CreateDelegate(info.EventHandlerType!, this);

            info.AddEventHandler(_resolvedSource, _eventHandler);
        }
        else if (!_isLoadedEventRegistered)
        {
            if (_resolvedSource is not FrameworkElement element || IsElementLoaded(element))
                return;
            _isLoadedEventRegistered = true;
            element.Loaded += OnEvent;
        }
    }

    private void UnregisterEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            return;
        }

        if (eventName != "Loaded")
        {
            if (_eventHandler is null)
            {
                return;
            }

            var info = _resolvedSource!.GetType().GetRuntimeEvent(eventName);
            info?.RemoveEventHandler(_resolvedSource, _eventHandler);

            _eventHandler = null;
        }
        else if (_isLoadedEventRegistered)
        {
            _isLoadedEventRegistered = false;
            var element = (FrameworkElement)_resolvedSource!;
            element.Loaded -= OnEvent;
        }
    }

    private void OnEvent(object sender, object eventArgs)
    {
        if (IsActive)
            _ = Interaction.ExecuteActions(_resolvedSource, Actions, eventArgs);
    }

    partial void OnSourceObjectPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        SetResolvedSource(ComputeResolvedSource());
    }

    partial void OnEventNamePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (AssociatedObject is null || _resolvedSource is null)
        {
            return;
        }

        var oldEventName = (string)e.OldValue;
        var newEventName = (string)e.NewValue;

        UnregisterEvent(oldEventName);
        RegisterEvent(newEventName);
    }

    internal static bool IsElementLoaded(FrameworkElement? element)
    {
        if (element is null)
        {
            return false;
        }

        var rootVisual = (UIElement?)null;
        if (element.XamlRoot is not null)
        {
            rootVisual = element.XamlRoot.Content;
        }

        var parent = element.Parent;

        // If the element is the child of a ControlTemplate it will have a null parent even when it is loaded.
        // To catch that scenario, also check its parent in the visual tree.
        parent ??= VisualTreeHelper.GetParent(element);

        return (parent is not null || (rootVisual is not null && element == rootVisual));
    }
}
