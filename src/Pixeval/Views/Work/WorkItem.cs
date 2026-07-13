// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Entry;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Work;

public class WorkItem : EntryItem, IWorkAnimatable
{
    public event EventHandler<Control, IWorkViewModel>? RequestOpenUserInfoPage;

    public event EventHandler<Control, IWorkViewModel>? RequestAddToBookmark;

    protected void AddToBookmark_OnRightClick(object sender, ContextRequestedEventArgs e)
    {
        RequestAddToBookmark?.Invoke(this, (IWorkViewModel) DataContext!);
    }

    protected void OpenUserInfoPage_OnClicked(object sender, RoutedEventArgs e)
    {
        RequestOpenUserInfoPage?.Invoke(this, (IWorkViewModel) DataContext!);
    }

    protected void OpenSeriesPage_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: IWorkViewModel { Entry: WorkBase { Series: { } series } work } }
            || TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;

        viewContainer.CreateSeriesPage(
            work is Novel ? SimpleWorkType.Novel : SimpleWorkType.Illustration,
            series.Id);
    }

    public void StartAnimation()
    {
        if (ElementComposition.GetElementVisual(this) is not { } visual)
            return;
        var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
        animation.InsertKeyFrame(0f, 0, new LinearEasing());
        animation.InsertKeyFrame(1f, 1, new LinearEasing());
        animation.Duration = TimeSpan.FromSeconds(0.3);
        visual.StartAnimation(nameof(CompositionVisual.Opacity), animation);
    }
}
