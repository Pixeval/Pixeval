// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;

namespace Pixeval.Controls;

public partial class DocumentViewer
{
    [GeneratedDependencyProperty]
    public partial NovelItemViewModel? NovelItem { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 1000d)]
    public partial double NovelMaxWidth { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 28d)]
    public partial double LineHeight { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 0)]
    public partial int CurrentPage { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 0)]
    public partial int PageCount { get; set; }

    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool IsMultiPage { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool LoadSuccessfully { get; set; }

    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool IsLoading { get; set; }

    [GeneratedDependencyProperty]
    public partial DocumentViewerViewModel? ViewModel { get; set; }
}
