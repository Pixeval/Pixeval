// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public partial class WorkSeriesInfoPane : UserControl
{
    private WorkSeriesInfoViewModel? ViewModel => DataContext as WorkSeriesInfoViewModel;

    public WorkSeriesInfoPane()
    {
        InitializeComponent();
        IsVisible = false;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        IsVisible = ViewModel is not null;
    }

    private void OpenSeriesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel
            || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        viewContainer.CreateSeriesPage(viewModel.WorkType, viewModel.Id);
    }

    private void NavigationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel
            || sender is not Control { Tag: WorkSeriesNavigationViewModel navigation }
            || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        if (viewModel.WorkType is SimpleWorkType.Novel)
            viewContainer.CreateNovelPage(navigation.Id);
        else if (navigation.Illustration is { } illustration)
            viewContainer.CreateIllustrationPage(new IllustrationItemViewModel(illustration));
        else
            viewContainer.CreateIllustrationPage(
                navigation.Id.ToString(),
                IPlatformInfo.Pixiv);
    }
}
