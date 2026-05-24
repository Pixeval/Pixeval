// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Media;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl : Button
{
    private readonly TextBlock _sizeTextBlock;
    private bool _ignoreNextClick;
    private PointerEditState? _pointerEditState;

    public HomePageCardControl(
        HomePageCardLayout card,
        HomeCardTemplate template,
        int rowCount,
        int columnCount,
        bool isEditing,
        bool isSelected)
    {
        Card = card;
        CardTemplate = template;
        RowCount = rowCount;
        ColumnCount = columnCount;
        IsEditing = isEditing;
        _sizeTextBlock = CreateSizeTextBlock();

        Padding = new(0);
        Background = Brushes.Transparent;
        BorderThickness = new(0);
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        Content = CreateContent(isSelected);
    }

    public event EventHandler<HomeCardSelectedEventArgs>? CardSelected;

    public event EventHandler<HomeCardEditPreviewEventArgs>? EditPreview;

    public event EventHandler<HomeCardEditCompletedEventArgs>? EditCompleted;

    public HomePageCardLayout Card { get; }

    public HomeCardTemplate CardTemplate { get; }

    public int RowCount { get; }

    public int ColumnCount { get; }

    public bool IsEditing { get; }

    public void CancelEdit()
    {
        CompleteEdit();
    }

    protected override void OnClick()
    {
        if (_ignoreNextClick)
        {
            _ignoreNextClick = false;
            base.OnClick();
            return;
        }

        if (IsEditing)
            CardSelected?.Invoke(this, new(Card, true));

        base.OnClick();
    }
}
