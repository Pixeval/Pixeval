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
        return makoClient.WorkRecommended(workType);
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
        return makoClient.WorkNew(workType);
    }
}

public class UserWorkPostsPage : WorkTypeWorksPage
{
    private readonly UserBasicInfo _user;

    public UserWorkPostsPage() : this(App.AppViewModel.MakoClient.Me!)
    {
    }

    public UserWorkPostsPage(UserBasicInfo user)
    {
        _user = user;
        Header = I18NManager.GetResource(EntryViewerPageResources.WorkNavigationViewItemContent);
        Icon = new SymbolIcon { Symbol = Symbol.Image, FontSize = 16, IconVariant = IconVariant.Color };
        EnableAddSubscriptionButton();
        ChangeSource();
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.WorkPosts(_user.Id, workType);
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
