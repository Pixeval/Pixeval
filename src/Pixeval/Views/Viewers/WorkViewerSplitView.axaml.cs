using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FluentIcons.Common;
using Pixeval.Utilities;

namespace Pixeval.Views.Viewers;

public partial class WorkViewerSplitView : SplitView
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SplitView);

    public static readonly StyledProperty<IEnumerable<NavigationInfo<double>>?> MenuItemsSourceProperty =
        AvaloniaProperty.Register<WorkViewerSplitView, IEnumerable<NavigationInfo<double>>?>(nameof(MenuItemsSource));

    public IEnumerable<NavigationInfo<double>>? MenuItemsSource
    {
        get => GetValue(MenuItemsSourceProperty);
        set => SetValue(MenuItemsSourceProperty, value);
    }

    static WorkViewerSplitView()
    {
        PaneBackgroundProperty.OverrideDefaultValue<WorkViewerSplitView>(Brushes.Transparent);
        DisplayModeProperty.OverrideDefaultValue<WorkViewerSplitView>(SplitViewDisplayMode.Inline);
    }

    public WorkViewerSplitView()
    {
        IsPaneOpen = true;
        MenuItemsSource =
        [
            new NavigationInfo<double>(typeof(int), Symbol.AccessTime, "test", 123),
            new NavigationInfo<double>(typeof(int), Symbol.Accessibility, "test2", 123)
        ];
        InitializeComponent();
    }
}
