// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using AnimatedControls.Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Pixeval.Controls;

public class AvatarImage : TemplatedControl
{
    public static readonly StyledProperty<IAnimatedBitmap?> SourceProperty = AvaloniaProperty.Register<AvatarImage, IAnimatedBitmap?>(nameof(Source), defaultValue: null);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty = AvaloniaProperty.Register<AvatarImage, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<AvatarImage, Stretch>(nameof(Stretch), Stretch.UniformToFill);

    public static readonly StyledProperty<bool> IsPlayingProperty = AvaloniaProperty.Register<AvatarImage, bool>(nameof(IsPlaying), true);

    [Content]
    public IAnimatedBitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }
}
