// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Mako.Global.Enum;

namespace Pixeval.Views.Work;

public static class BookmarkTagSelectorFlyoutHelper
{
    public static async Task ShowAsync(
        Control placementTarget,
        SimpleWorkType workType,
        Func<(bool isPrivate, IReadOnlyList<string> tags), Task> onTagsSelected,
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

        async void OnTagsSelected(TagSelector sender, (bool isPrivate, IReadOnlyList<string> tags) e)
        {
            await onTagsSelected(e);
            flyout.Hide();
        }
    }
}
