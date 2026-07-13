// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Entry;

public partial class SeriesItem : EntryItem
{
    public SeriesItem() => InitializeComponent();

    private void LatestContentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SeriesItemViewModel viewModel }
            || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        if (viewModel.WorkType is SimpleWorkType.Novel)
            viewContainer.CreateNovelPage(viewModel.Entry.LatestContentId);
        else
            viewContainer.CreateIllustrationPage(
                viewModel.Entry.LatestContentId.ToString(CultureInfo.InvariantCulture),
                IPlatformInfo.Pixiv);
    }

    private void AuthorButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SeriesItemViewModel viewModel })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        viewContainer.CreateUserPage(viewModel.Entry.User.Id);
    }
}
