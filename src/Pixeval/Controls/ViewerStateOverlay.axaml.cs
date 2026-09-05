// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls.Primitives;

namespace Pixeval.Controls;

public sealed class ViewerStateOverlay : TemplatedControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<ViewerStateOverlay, bool>(nameof(IsLoading));

    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.Register<ViewerStateOverlay, string?>(nameof(ErrorMessage));

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string? ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }
}
