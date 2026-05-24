// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;

namespace Pixeval.Views.Home;

public sealed partial class HomeCardSinglePreview : UserControl
{
    public HomeCardSinglePreview()
    {
        InitializeComponent();
    }

    public HomeCardSinglePreview(HomeCardPreviewCell cell) : this()
    {
        DataContext = cell;
    }
}
