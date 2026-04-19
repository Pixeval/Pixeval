// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Misaki;

namespace Pixeval.Views.Viewers;

public partial class WorkInfoPage : ContentPage
{
    public WorkInfoPage() : this(null)
    {
    }

    public WorkInfoPage(IArtworkInfo? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
