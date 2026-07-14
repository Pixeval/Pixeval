// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl
{
    private const int MinimumSpan = 1;

    private const string PressedClass = "pressed";

    private Border? _pressedResizeHandle;

    private void Card_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEditing
            || IsQuickDeleteEvent(e)
            || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        BeginEdit(HomeCardEditAction.Move, RootPanel, e);
        CardSelected?.Invoke(this, new(Card));
        e.Handled = true;
    }

    private void ResizeHandle_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Border { Tag: HomeCardEditAction action }
            || !IsEditing
            || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        SetPressedResizeHandle((Border) e.Source);
        BeginEdit(action, RootPanel, e);
        CardSelected?.Invoke(this, new(Card));
        e.Handled = true;
    }

    private bool IsQuickDeleteEvent(PointerPressedEventArgs e) =>
        e.Source is Visual visual
        && visual.GetSelfAndVisualAncestors().Contains(QuickDeleteButton);

    private void BeginEdit(HomeCardEditAction action, Control captureTarget, PointerPressedEventArgs e)
    {
        if (Parent is not Grid layoutGrid)
            return;

        _pointerEditState = new(
            action,
            e.Pointer,
            layoutGrid,
            e.GetPosition(layoutGrid),
            Card.Column,
            Card.Row,
            Card.ColumnSpan,
            Card.RowSpan);
        e.Pointer.Capture(captureTarget);
        e.Handled = true;
    }

    private void Card_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pointerEditState is null || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed is false)
            return;

        var candidate = CreateCandidate(e);
        if (candidate.Equals(HomeCardBounds.From(Card)))
        {
            e.Handled = true;
            return;
        }

        var previewEventArgs = new HomeCardEditPreviewEventArgs(Card, candidate);
        EditPreview?.Invoke(this, previewEventArgs);
        if (previewEventArgs.Accepted)
        {
            ApplyBounds(candidate);
            _pointerEditState.HasChanged = true;
        }

        e.Handled = true;
    }

    private void Card_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_pointerEditState is null)
            return;

        CompleteEdit();
        e.Handled = true;
    }

    private void Card_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_pointerEditState is not null)
            CompleteEdit();
    }

    private HomeCardBounds CreateCandidate(PointerEventArgs e)
    {
        if (_pointerEditState is not { } state)
            return HomeCardBounds.From(Card);

        var pointerPosition = e.GetPosition(state.LayoutGrid);
        var deltaColumn = GetGridDelta(pointerPosition.X - state.StartPoint.X, state.LayoutGrid.Bounds.Width, state.LayoutGrid.ColumnSpacing, ColumnCount);
        var deltaRow = GetGridDelta(pointerPosition.Y - state.StartPoint.Y, state.LayoutGrid.Bounds.Height, state.LayoutGrid.RowSpacing, RowCount);
        var left = state.StartColumn;
        var top = state.StartRow;
        var right = state.StartColumn + state.StartColumnSpan;
        var bottom = state.StartRow + state.StartRowSpan;

        switch (state.Action)
        {
            case HomeCardEditAction.Move:
                left = state.StartColumn + deltaColumn;
                top = state.StartRow + deltaRow;
                right = left + state.StartColumnSpan;
                bottom = top + state.StartRowSpan;
                break;
            case HomeCardEditAction.ResizeLeft:
                left += deltaColumn;
                break;
            case HomeCardEditAction.ResizeTop:
                top += deltaRow;
                break;
            case HomeCardEditAction.ResizeRight:
                right += deltaColumn;
                break;
            case HomeCardEditAction.ResizeBottom:
                bottom += deltaRow;
                break;
            case HomeCardEditAction.ResizeTopLeft:
                left += deltaColumn;
                top += deltaRow;
                break;
            case HomeCardEditAction.ResizeTopRight:
                right += deltaColumn;
                top += deltaRow;
                break;
            case HomeCardEditAction.ResizeBottomRight:
                right += deltaColumn;
                bottom += deltaRow;
                break;
            case HomeCardEditAction.ResizeBottomLeft:
                left += deltaColumn;
                bottom += deltaRow;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        EnsureMinimumSize(state.Action, ref left, ref top, ref right, ref bottom);
        ClampToGrid(state.Action, ref left, ref top, ref right, ref bottom);
        return new(left, top, right - left, bottom - top);
    }

    private static void EnsureMinimumSize(HomeCardEditAction action, ref int left, ref int top, ref int right, ref int bottom)
    {
        if (right - left < MinimumSpan)
        {
            if (action is HomeCardEditAction.ResizeLeft or HomeCardEditAction.ResizeTopLeft or HomeCardEditAction.ResizeBottomLeft)
                left = right - MinimumSpan;
            else
                right = left + MinimumSpan;
        }

        if (bottom - top >= MinimumSpan)
            return;

        if (action is HomeCardEditAction.ResizeTop or HomeCardEditAction.ResizeTopLeft or HomeCardEditAction.ResizeTopRight)
            top = bottom - MinimumSpan;
        else
            bottom = top + MinimumSpan;
    }

    private void ClampToGrid(HomeCardEditAction action, ref int left, ref int top, ref int right, ref int bottom)
    {
        if (action is HomeCardEditAction.Move)
        {
            var width = right - left;
            var height = bottom - top;
            left = int.Clamp(left, 0, ColumnCount - width);
            top = int.Clamp(top, 0, RowCount - height);
            right = left + width;
            bottom = top + height;
            return;
        }

        left = int.Clamp(left, 0, ColumnCount - MinimumSpan);
        top = int.Clamp(top, 0, RowCount - MinimumSpan);
        right = int.Clamp(right, left + MinimumSpan, ColumnCount);
        bottom = int.Clamp(bottom, top + MinimumSpan, RowCount);
    }

    private void ApplyBounds(HomeCardBounds bounds)
    {
        bounds.ApplyTo(Card);

        Grid.SetColumn(this, Card.Column);
        Grid.SetRow(this, Card.Row);
        Grid.SetColumnSpan(this, Card.ColumnSpan);
        Grid.SetRowSpan(this, Card.RowSpan);
    }

    private void CompleteEdit(bool raiseEvent = true)
    {
        if (_pointerEditState is null)
            return;

        var state = _pointerEditState;
        _pointerEditState = null;
        SetPressedResizeHandle(null);
        state.Pointer.Capture(null);
        if (raiseEvent)
            EditCompleted?.Invoke(this, new(Card, state.HasChanged));
    }

    private void SetPressedResizeHandle(Border? handle)
    {
        if (_pressedResizeHandle == handle)
            return;

        _pressedResizeHandle?.Classes.Remove(PressedClass);
        _pressedResizeHandle = handle;
        _pressedResizeHandle?.Classes.Add(PressedClass);
    }

    private static int GetGridDelta(double delta, double extent, double spacing, int count)
    {
        if (extent <= 0 || count <= 0)
            return 0;

        var cellExtent = int.Max(1, (int) (extent - (spacing * int.Max(0, count - 1))) / count);
        return (int) double.Round(delta / (cellExtent + spacing), MidpointRounding.AwayFromZero);
    }

    private sealed class PointerEditState(
        HomeCardEditAction action,
        IPointer pointer,
        Grid layoutGrid,
        Point startPoint,
        int startColumn,
        int startRow,
        int startColumnSpan,
        int startRowSpan)
    {
        public HomeCardEditAction Action { get; } = action;

        public IPointer Pointer { get; } = pointer;

        public Grid LayoutGrid { get; } = layoutGrid;

        public Point StartPoint { get; } = startPoint;

        public int StartColumn { get; } = startColumn;

        public int StartRow { get; } = startRow;

        public int StartColumnSpan { get; } = startColumnSpan;

        public int StartRowSpan { get; } = startRowSpan;

        public bool HasChanged { get; set; }
    }
}
