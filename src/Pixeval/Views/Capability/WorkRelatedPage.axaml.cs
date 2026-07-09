// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Linq;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class WorkRelatedPage : IconContentPage
{
    private readonly long _id;
    private readonly SimpleWorkType _simpleWorkType;

    public WorkRelatedPage() : this(0, PixevalSettings.SimpleWorkType)
    {
    }

    public WorkRelatedPage(long id, SimpleWorkType simpleWorkType, IWorkViewViewModel? viewModel = null)
    {
        _id = id;
        _simpleWorkType = simpleWorkType;
        InitializeComponent();
        if (viewModel is not null)
            WorkContainer.SetViewModel(viewModel);
        else
            ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        var engine = _id is 0
            ? App.AppViewModel.MakoClient.Computed(AsyncEnumerable.Empty<IWorkEntry>())
            : App.AppViewModel.MakoClient.WorkRelated(_id, _simpleWorkType);
        WorkContainer.ResetEngine(engine);
    }
}
