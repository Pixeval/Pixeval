#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/CardControl.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.Card;

/// <summary>
/// This is the base control to create consistent settings experiences, inline with the Windows 11 design language.
/// A CardControl can also be hosted within a SettingsExpander.
/// </summary>
[DependencyProperty<bool>("IsClickEnabled", "false", nameof(OnIsClickEnabledChanged))]
[DependencyProperty<bool>("IsSelectEnabled", "false", nameof(OnIsSelectEnabledChanged))]
[DependencyProperty<bool>("IsSelected", "false", nameof(OnIsSelectedChanged))]
public partial class CardControl : ButtonBase
{
    public event TypedEventHandler<CardControl, CancellableEventArgs>? IsSelectedChanging;
    public event TypedEventHandler<CardControl, EventArgs>? IsSelectedChanged;

    internal const string NormalState = "Normal";
    internal const string PointerOverState = "PointerOver";
    internal const string PressedState = "Pressed";
    internal const string DisabledState = "Disabled";
    internal const string SelectedState = "Selected";

    /// <summary>
    /// Creates a new instance of the <see cref="CardControl"/> class.
    /// </summary>
    public CardControl() => DefaultStyleKey = typeof(CardControl);

    private static void OnIsClickEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CardControl)d).OnIsClickEnabledPropertyChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static void OnIsSelectEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CardControl)d).OnIsSelectEnabledPropertyChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CardControl)d).OnIsSelectedPropertyChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        ChangeSelectionState(IsEnabled ? NormalState : DisabledState);
    }

    protected virtual void OnIsClickEnabledPropertyChanged(bool oldValue, bool newValue)
    {
        OnEnabledModeChanged();
    }

    protected virtual void OnIsSelectEnabledPropertyChanged(bool oldValue, bool newValue)
    {
        OnEnabledModeChanged();
    }

    protected virtual void OnIsSelectedPropertyChanged(bool oldValue, bool newValue)
    {
        ChangeSelectionState();
    }

    private bool IsOperationEnabled => IsClickEnabled || IsSelectEnabled;

    private string _currentState = NormalState;

    private void ChangeSelectionState(string? newState = null)
    {
        if (newState is not null)
            _currentState = newState;
        else
            newState = _currentState;

        _ = VisualStateManager.GoToState(this, newState + (IsSelected ? SelectedState : ""), true);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        IsEnabledChanged -= OnIsEnabledChanged;
        OnEnabledModeChanged();
        ChangeSelectionState(IsEnabled ? NormalState : DisabledState);
        IsEnabledChanged += OnIsEnabledChanged;
    }

    private bool _isInteractionEnabled;

    private void EnableButtonInteraction()
    {
        DisableButtonInteraction();

        _isInteractionEnabled = IsTabStop = IsTapEnabled = true;
        PointerEntered += Control_PointerEntered;
        PointerExited += Control_PointerExited;
        PointerCaptureLost += Control_PointerCaptureLost;
        PointerCanceled += Control_PointerCanceled;
        PreviewKeyDown += Control_PreviewKeyDown;
        Tapped += Control_Tapped;
    }

    private void DisableButtonInteraction()
    {
        _isInteractionEnabled = IsTabStop = IsTapEnabled = false;
        PointerEntered -= Control_PointerEntered;
        PointerExited -= Control_PointerExited;
        PointerCaptureLost -= Control_PointerCaptureLost;
        PointerCanceled -= Control_PointerCanceled;
        PreviewKeyDown -= Control_PreviewKeyDown;
        PreviewKeyUp -= Control_PreviewKeyUp;
        Tapped -= Control_Tapped;
    }

    private void Control_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (IsSelectEnabled && IsEnabled)
        {
            var eventArgs = new CancellableEventArgs();
            IsSelectedChanging?.Invoke(this, eventArgs);
            if (eventArgs.Cancel)
                return;
            IsSelected = !IsSelected;
            IsSelectedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Control_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is Windows.System.VirtualKey.Enter or Windows.System.VirtualKey.Space or Windows.System.VirtualKey.GamepadA)
        {
            ChangeSelectionState(NormalState);
        }
    }

    private void Control_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is Windows.System.VirtualKey.Enter or Windows.System.VirtualKey.Space or Windows.System.VirtualKey.GamepadA)
        {
            // Check if the active focus is on the card itself - only then we show the pressed state.
            if (GetFocusedElement() is CardControl)
            {
                ChangeSelectionState(PressedState);
            }
        }
    }

    public void Control_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);
        ChangeSelectionState(PointerOverState);
    }

    public void Control_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        ChangeSelectionState(NormalState);
    }

    private void Control_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        ChangeSelectionState(NormalState);
    }

    private void Control_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        base.OnPointerCanceled(e);
        ChangeSelectionState(NormalState);
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        //  e.Handled = true;
        if (IsOperationEnabled)
        {
            base.OnPointerPressed(e);
            ChangeSelectionState(PressedState);
        }
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        if (IsOperationEnabled)
        {
            base.OnPointerReleased(e);
            ChangeSelectionState(NormalState);
        }
    }

    /// <summary>
    /// Creates AutomationPeer
    /// </summary>
    /// <returns>An automation peer for <see cref="CardControl"/>.</returns>
    protected override AutomationPeer OnCreateAutomationPeer() => new CardControlAutomationPeer(this);

    private void OnEnabledModeChanged()
    {
        if (IsOperationEnabled)
        {
            if (!_isInteractionEnabled)
                EnableButtonInteraction();
        }
        else
        {
            DisableButtonInteraction();
        }
    }

    private FrameworkElement? GetFocusedElement()
    {
        return XamlRoot is not null
            ? FocusManager.GetFocusedElement(XamlRoot) as FrameworkElement
            : FocusManager.GetFocusedElement() as FrameworkElement;
    }
}
