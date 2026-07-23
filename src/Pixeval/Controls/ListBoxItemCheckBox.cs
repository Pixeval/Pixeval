// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Pixeval.Controls;

public class ListBoxItemCheckBox : CheckBox
{
    private bool _isRangeSelection;

    public ListBoxItemCheckBox()
    {
        Tapped += (_, e) => e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _isRangeSelection = false;
        ForwardRangeSelection(e);
        base.OnPointerPressed(e);
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ForwardRangeSelection(e);
        base.OnPointerReleased(e);
        _isRangeSelection = false;
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _isRangeSelection = false;
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (_isRangeSelection)
            return;

        base.OnClick();
    }

    private void ForwardRangeSelection(PointerEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift)
            || this.FindAncestorOfType<ListBoxItem>() is not { } item
            || ItemsControl.ItemsControlFromItemContainer(item) is not ListBox listBox)
            return;

        _isRangeSelection |= listBox.UpdateSelectionFromEvent(item, e);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CheckBox);
}
