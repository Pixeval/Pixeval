// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace Pixeval.Views.Markup;

/// <summary>
/// A bindable-key version of <see cref="Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension"/>.
/// Supports both static resource keys and binding-supplied keys.
/// <para>
/// Usage:
/// <code>
/// &lt;TextBlock Text="{local:BindingDynamicResource ResourceKey={Binding MyKeyProperty}}" /&gt;
/// &lt;TextBlock Text="{local:BindingDynamicResource MyStaticKey}" /&gt;
/// </code>
/// </para>
/// </summary>
public sealed class BindingDynamicResourceExtension : MarkupExtension
{
    public BindingDynamicResourceExtension()
    {
    }

    public BindingDynamicResourceExtension(object resourceKey) => ResourceKey = resourceKey;

    [ConstructorArgument("resourceKey")]
    public object? ResourceKey { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var provideTarget = (IProvideValueTarget?)serviceProvider.GetService(typeof(IProvideValueTarget));
        if (provideTarget?.TargetObject is not StyledElement target ||
            provideTarget.TargetProperty is not AvaloniaProperty targetProperty)
            return AvaloniaProperty.UnsetValue;

        if (ResourceKey is BindingBase keyBinding)
        {
            var proxy = new ResourceKeyProxy(target, targetProperty);
            proxy.Bind(ResourceKeyProxy.ResolvedKeyProperty, keyBinding);
            proxy[!StyledElement.DataContextProperty] = target[!StyledElement.DataContextProperty];
            return AvaloniaProperty.UnsetValue;
        }

        if (ResourceKey is not null)
            return target.GetResourceObservable(ResourceKey).ToBinding();

        return AvaloniaProperty.UnsetValue;
    }
}

/// <summary>
/// Lightweight proxy used to resolve a key binding outside the visual tree.
/// DataContext is manually synced from the real target element.
/// </summary>
internal sealed class ResourceKeyProxy : StyledElement
{
    public static readonly StyledProperty<object?> ResolvedKeyProperty =
        AvaloniaProperty.Register<ResourceKeyProxy, object?>(nameof(ResolvedKey));

    private readonly StyledElement _target;
    private readonly AvaloniaProperty _targetProperty;
    private IDisposable? _currentResourceBinding;

    public ResourceKeyProxy(StyledElement target, AvaloniaProperty targetProperty)
    {
        _target = target;
        _targetProperty = targetProperty;
    }

    public object? ResolvedKey
    {
        get => GetValue(ResolvedKeyProperty);
        set => SetValue(ResolvedKeyProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ResolvedKeyProperty)
        {
            _currentResourceBinding?.Dispose();
            _currentResourceBinding = change.NewValue is { } key
                ? _target.Bind(_targetProperty, _target.GetResourceObservable(key).ToBinding())
                : null;
        }
    }
}
