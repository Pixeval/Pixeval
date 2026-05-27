// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Controls;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.ViewModels;

namespace Pixeval.Views.Search;

public partial class WorkSearchPage : ContentPage
{
    public WorkSearchPage() : this("")
    {
    }

    public WorkSearchPage(string searchText, SimpleWorkType preferredType) : this(
        searchText,
        new IllustrationSearchArguments(searchText),
        new NovelSearchArguments(searchText),
        preferredType)
    {
    }

    public WorkSearchPage(IllustrationSearchArguments illustrationSearchArguments)
        : this(illustrationSearchArguments.SearchText, illustrationSearchArguments)
    {
    }

    public WorkSearchPage(NovelSearchArguments novelSearchArguments)
        : this(novelSearchArguments.SearchText, null, novelSearchArguments, SimpleWorkType.Novel)
    {
    }

    public WorkSearchPage(
        string searchText,
        IllustrationSearchArguments? illustrationSearchArguments = null,
        NovelSearchArguments? novelSearchArguments = null,
        SimpleWorkType preferredType = default,
        IWorkViewViewModel? viewModel = null)
    {
        InitializeComponent();
        Header = I18NManager.GetResource(MainPageResources.SearchResultFormatted, searchText);
        SimpleWorkTypeComboBox.SelectedValue = preferredType;
        _illustrationArguments = illustrationSearchArguments;
        _novelArguments = novelSearchArguments;
        SetIsSwitchEnabled();
        if (viewModel is not null)
            WorkContainer.SetViewModel(viewModel);
        else
            ChangeSource();
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        var engine = (_illustrationArguments, _novelArguments) switch
        {
            (null, null) => App.AppViewModel.MakoClient.Computed(AsyncEnumerable.Empty<IWorkEntry>()),
            (_, null) => App.AppViewModel.MakoClient.IllustrationSearch(_illustrationArguments),
            (null, _) => App.AppViewModel.MakoClient.NovelSearch(_novelArguments),
            _ => SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>() switch
            {
                SimpleWorkType.Novel => App.AppViewModel.MakoClient.NovelSearch(_novelArguments),
                _ => App.AppViewModel.MakoClient.IllustrationSearch(_illustrationArguments),
            }
        };

        WorkContainer.ResetEngine(engine);
    }

    private readonly IllustrationSearchArguments? _illustrationArguments;

    private readonly NovelSearchArguments? _novelArguments;

    private void SetIsSwitchEnabled()
    {
        if (_illustrationArguments is null || _novelArguments is null)
            return;
        SimpleWorkTypeComboBox.IsEnabled = true;
        SimpleWorkTypeComboBox.IsVisible = true;
    }
}
