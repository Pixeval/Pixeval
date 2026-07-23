// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Pixeval.Utilities;

namespace Pixeval.Views.Markup;

public sealed class PlatformCommandGestureExtension : MarkupExtension
{
    public PlatformCommandGestureExtension()
    {
    }

    public PlatformCommandGestureExtension(Key key)
    {
        Key = key;
    }

    [ConstructorArgument("key")]
    public Key Key { get; set; }

    public KeyModifiers AdditionalModifiers { get; set; }

    /// <inheritdoc />
    public override KeyGesture ProvideValue(IServiceProvider serviceProvider) =>
        KeyboardShortcut.CreatePlatformCommandGesture(Key, AdditionalModifiers);
}
