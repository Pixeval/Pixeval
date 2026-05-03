using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Mako;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.I18N;

namespace Pixeval.Views.Capability;

public abstract partial class WorkTypeWorksPage : ContentPage
{
    public WorkTypeWorksPage()
    {
        InitializeComponent();
    }

    private void WorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e)
    {
        ChangeSource();
    }

    protected void ChangeSource()
    {
        WorkContainer.ResetEngine(GetFetchEngine(App.AppViewModel.MakoClient, WorkTypeComboBox.GetSelectedValue<WorkType>()));
    }

    protected abstract IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType);
}

public class RecommendWorksPage : WorkTypeWorksPage
{
    public RecommendWorksPage()
    {
        Header = I18NManager.GetResource(MainPageResources.RecommendationsTabContent);
        Icon = new SymbolIcon { Symbol = Symbol.Calendar, FontSize = 16, IconVariant = IconVariant.Color };
        ChangeSource();
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.RecommendedWorks(workType);
    }
}

public class NewWorksPage : WorkTypeWorksPage
{
    public NewWorksPage()
    {
        Header = I18NManager.GetResource(MainPageResources.NewWorksTabContent);
        Icon = new SymbolIcon { Symbol = Symbol.ArrowSync, FontSize = 16, IconVariant = IconVariant.Color };
        ChangeSource();
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.NewWorks(workType);
    }
}

public class UserWorkPostsPage : WorkTypeWorksPage
{
    private readonly long _userId;

    public UserWorkPostsPage() : this(App.AppViewModel.PixivUid)
    {
    }

    public UserWorkPostsPage(long id)
    {
        _userId = id;
        Header = I18NManager.GetResource(EntryViewerPageResources.WorkNavigationViewItemContent);
        Icon = new SymbolIcon { Symbol = Symbol.Image, FontSize = 16, IconVariant = IconVariant.Color };
        ChangeSource();
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.WorkPosts(_userId, workType);
    }
}
