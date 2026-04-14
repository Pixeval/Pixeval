// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Pixeval.I18N;

namespace Pixeval.Controls;

public class PixevalBadge : TemplatedControl
{
    public static readonly StyledProperty<bool> UseSmallProperty =
        AvaloniaProperty.Register<PixevalBadge, bool>(nameof(UseSmall));

    public static readonly StyledProperty<BadgeMode> ModeProperty =
        AvaloniaProperty.Register<PixevalBadge, BadgeMode>(nameof(Mode));

    public static readonly DirectProperty<PixevalBadge, string> TextProperty =
        AvaloniaProperty.RegisterDirect<PixevalBadge, string>(nameof(Text), o => o.Text);

    private static readonly Dictionary<BadgeMode, string> _PropertySet = new()
    {
        [BadgeMode.None] = "None",
        [BadgeMode.Premium] = "Premium",
        [BadgeMode.Following] = I18NManager.GetResource(PixevalBadgeResources.Following),
        [BadgeMode.Gif] = "GIF",
        [BadgeMode.R18] = "R18",
        [BadgeMode.R18G] = "R18G",
        [BadgeMode.Ai] = "AI"
    };

    static PixevalBadge()
    {
        ModeProperty.Changed.AddClassHandler<PixevalBadge>(static (control, e) =>
        {
            control.UpdateFromMode((BadgeMode)e.NewValue!);
        });
    }

    public PixevalBadge()
    {
        UpdateFromMode(Mode);
    }

    public bool UseSmall
    {
        get => GetValue(UseSmallProperty);
        set => SetValue(UseSmallProperty, value);
    }

    public BadgeMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public string Text
    {
        get;
        private set => SetAndRaise(TextProperty, ref field, value);
    } = "";

    private void UpdateFromMode(BadgeMode mode)
    {
        if (!_PropertySet.TryGetValue(mode, out var definition))
        {
            definition = _PropertySet[BadgeMode.None];
        }

        Text = definition;
    }
}
