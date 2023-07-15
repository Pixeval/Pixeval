using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using WinUI3Utilities.Attributes;

namespace Pixeval.Util.Triggers;

/// <summary>
/// <seealso href="https://github.com/microsoft/XamlBehaviors/blob/master/src/BehaviorsSDKManaged/Microsoft.Xaml.Interactions.Shared/Core/EventTriggerBehavior.cs"/>
/// </summary>
[DependencyProperty<string>("EventName", "\"Loaded\"", nameof(OnEventNameChanged))]
[DependencyProperty<object>("SourceObject", DependencyPropertyDefaultValue.Default, nameof(OnSourceObjectChanged))]
[DependencyProperty<bool>("IsActive", "true")]
public partial class EventWithConditionTriggerBehavior : Trigger
{
    private object? _resolvedSource;
    private Delegate? _eventHandler;
    private bool _isLoadedEventRegistered;

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

    private object ComputeResolvedSource()
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

    private static void OnSourceObjectChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (EventWithConditionTriggerBehavior)dependencyObject;
        behavior.SetResolvedSource(behavior.ComputeResolvedSource());
    }

    private static void OnEventNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        var behavior = (EventWithConditionTriggerBehavior)dependencyObject;
        if (behavior.AssociatedObject is null || behavior._resolvedSource is null)
        {
            return;
        }

        var oldEventName = (string)args.OldValue;
        var newEventName = (string)args.NewValue;

        behavior.UnregisterEvent(oldEventName);
        behavior.RegisterEvent(newEventName);
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
        else if (Window.Current is not null)
        {
            rootVisual = Window.Current.Content;
        }

        var parent = element.Parent;

        // If the element is the child of a ControlTemplate it will have a null parent even when it is loaded.
        // To catch that scenario, also check it's parent in the visual tree.
        parent ??= VisualTreeHelper.GetParent(element);

        return (parent is not null || (rootVisual is not null && element == rootVisual));
    }
}
