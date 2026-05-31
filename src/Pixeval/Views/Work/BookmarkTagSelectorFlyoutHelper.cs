// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Mako.Global.Enum;

namespace Pixeval.Views.Work;

public static class BookmarkTagSelectorFlyoutHelper
{
    public static async Task ShowAsync(
        Control placementTarget,
        SimpleWorkType workType,
        Func<(bool IsPrivate, IReadOnlyList<string>? Tags), Task> onTagsSelected,
        PlacementMode placement = PlacementMode.Bottom)
    {
        var flyout = new Flyout
        {
            Placement = placement
        };
        var tagSelector = new TagSelector
        {
            WorkType = workType
        };
        flyout.Content = new Border
        {
            Width = 340,
            MaxHeight = 420,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            Child = tagSelector
        };
        tagSelector.TagsSelected += OnTagsSelected;
        flyout.Closed += (_, _) => tagSelector.TagsSelected -= OnTagsSelected;
        flyout.ShowAt(placementTarget);
        await tagSelector.ResetSourceAsync();
        return;

        async void OnTagsSelected(TagSelector sender, (bool IsPrivate, IReadOnlyList<string>? Tags) e)
        {
            await onTagsSelected(e);
            flyout.Hide();
        }
    }
}
