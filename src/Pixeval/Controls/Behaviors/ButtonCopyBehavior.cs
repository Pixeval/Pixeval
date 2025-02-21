// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public partial class ButtonCopyBehavior : Behavior<Button>
{
    [GeneratedDependencyProperty]
    public partial string? TargetText { get; set; } 

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Click += OnClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Click -= OnClick;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(TargetText))
            return;
        UiHelper.ClipboardSetText(TargetText);
    }
}
