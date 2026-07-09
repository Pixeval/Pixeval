// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

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
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public abstract partial class WorkTypeWorksPage : ContentPage
{
    protected WorkTypeWorksPage(string header, Symbol symbol)
    {
        Header = header;
        Icon = new SymbolIcon { Symbol = symbol, FontSize = 16, IconVariant = IconVariant.Color };
        InitializeComponent();
    }

    protected void InitializeSource(WorkType workType, IWorkViewViewModel? viewModel = null)
    {
        WorkTypeComboBox.SelectedValue = workType;

        if (viewModel is not null)
            WorkContainer.SetViewModel(viewModel);
        else
            ChangeSource();
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
        var workType = WorkTypeComboBox.GetSelectedValue<WorkType>();
        var engine = GetFetchEngine(App.AppViewModel.MakoClient, workType);
        WorkContainer.ResetEngine(engine);
        OnSourceChanged(engine, workType);
    }

    protected abstract IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType);

    protected virtual void OnSourceChanged(IFetchEngine<IWorkEntry> engine, WorkType workType)
    {
    }

    protected void EnableAddSubscriptionButton() => AddSubscriptionButton.IsVisible = true;

    private void AddSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e) => AddSubscription();

    protected virtual void AddSubscription()
    {
    }
}

public class WorkRecommendedPage : WorkTypeWorksPage
{
    public WorkRecommendedPage() : this(PixevalSettings.WorkType)
    {
    }

    public WorkRecommendedPage(WorkType workType, IWorkViewViewModel? viewModel = null) : base(I18NManager.GetResource(MainPageResources.TabWorkRecommended), Symbol.Calendar)
    {
        InitializeSource(workType, viewModel);
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.WorkRecommended(workType);
    }
}

public class WorkNewPage : WorkTypeWorksPage
{
    public WorkNewPage() : this(PixevalSettings.WorkType)
    {
    }

    public WorkNewPage(WorkType workType, IWorkViewViewModel? viewModel = null) : base(I18NManager.GetResource(MainPageResources.TabWorkNew), Symbol.ArrowSync)
    {
        InitializeSource(workType, viewModel);
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.WorkNew(workType);
    }
}

public class WorkPostsPage : WorkTypeWorksPage
{
    private readonly UserBasicInfo _user;

    public WorkPostsPage() : this(PixevalSettings.Me)
    {
    }

    public WorkPostsPage(UserBasicInfo user) : this(user, PixevalSettings.WorkType)
    {
    }

    public WorkPostsPage(UserBasicInfo user, WorkType workType, IWorkViewViewModel? viewModel = null) : base(string.Format(I18NManager.GetResource(MainPageResources.TabWorkPosts)), Symbol.Image)
    {
        _user = user;
        EnableAddSubscriptionButton();
        InitializeSource(workType, viewModel);
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.WorkPosted(_user.Id, workType);
    }

    protected override void OnSourceChanged(IFetchEngine<IWorkEntry> engine, WorkType workType)
    {
        App.AppViewModel.QueueWorkSubscriptionSyncCurrentSource(
            _user.Id,
            WorkSubscriptionType.Posts,
            workType switch
            {
                WorkType.Illustration => WorkSubscriptionWorkKind.Illustration,
                WorkType.Manga => WorkSubscriptionWorkKind.Manga,
                WorkType.Novel => WorkSubscriptionWorkKind.Novel,
                _ => throw new ArgumentOutOfRangeException(nameof(workType))
            },
            engine);
    }

    protected override void AddSubscription()
    {
        var workType = WorkTypeComboBox.GetSelectedValue<WorkType>();
        var workKind = workType switch
        {
            WorkType.Illustration => WorkSubscriptionWorkKind.Illustration,
            WorkType.Manga => WorkSubscriptionWorkKind.Manga,
            WorkType.Novel => WorkSubscriptionWorkKind.Novel,
            _ => throw new ArgumentOutOfRangeException(nameof(workType))
        };

        if (WorkSubscriptionHelper.TryAddOrUpdate(_user, WorkSubscriptionType.Posts, workKind))
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.SubscriptionAdded));
    }
}
