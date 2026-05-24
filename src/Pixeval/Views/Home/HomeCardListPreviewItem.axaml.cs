// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;

namespace Pixeval.Views.Home;

public sealed partial class HomeCardListPreviewItem : UserControl
{
    public HomeCardListPreviewItem()
    {
        InitializeComponent();
    }

    public HomeCardListPreviewItem(HomeCardPreviewCell cell) : this()
    {
        DataContext = cell;
    }
}
