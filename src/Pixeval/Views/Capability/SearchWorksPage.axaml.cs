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

namespace Pixeval.Views.Capability;

public partial class SearchWorksPage : ContentPage
{
    public SearchWorksPage() : this("", null, null, default)
    {
    }

    public SearchWorksPage(string searchText) : this(searchText, null, null, default)
    {
    }

    public SearchWorksPage(string searchText, SimpleWorkType preferredType) : this(
        searchText,
        new IllustrationSearchArguments(searchText),
        new NovelSearchArguments(searchText),
        preferredType)
    {
    }

    public SearchWorksPage(IllustrationSearchArguments illustrationSearchArguments)
        : this(illustrationSearchArguments.SearchText, illustrationSearchArguments, null, SimpleWorkType.IllustrationAndManga)
    {
    }

    public SearchWorksPage(NovelSearchArguments novelSearchArguments)
        : this(novelSearchArguments.SearchText, null, novelSearchArguments, SimpleWorkType.Novel)
    {
    }

    public SearchWorksPage(
        string searchText,
        IllustrationSearchArguments? illustrationSearchArguments,
        NovelSearchArguments? novelSearchArguments,
        SimpleWorkType preferredType)
    {
        InitializeComponent();
        Header = I18NManager.GetResource(MainPageResources.SearchResultFormatted, searchText);
        SimpleWorkTypeComboBox.SelectedValue = preferredType;
        _illustrationArguments = illustrationSearchArguments;
        _novelArguments = novelSearchArguments;
        SetIsSwitchEnabled();
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
