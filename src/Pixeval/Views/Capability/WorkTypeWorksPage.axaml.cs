// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Work;

namespace Pixeval.Views.Capability;

public abstract partial class WorkTypeWorksPage : IconContentPage
{
    protected WorkTypeWorksPage() => InitializeComponent();

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

    protected static IWorkSubscriptionService SubscriptionService =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<IWorkSubscriptionService>();

    protected void EnableAddSubscriptionButton() => AddSubscriptionButton.IsVisible = true;

    protected void UpdateSubscriptionButtons(
        long targetId,
        WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind) =>
        WorkSubscriptionButtonHelper.UpdateVisibility(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            SubscriptionService.TryGetSubscription(targetId, subscriptionType, workKind) is not null);

    protected Task<TResult> RunSubscriptionOperationAsync<TResult>(
        Func<Task<TResult>> operation,
        Action updateButtons) =>
        WorkSubscriptionButtonHelper.RunAsync(
            AddSubscriptionButton,
            RemoveSubscriptionButton,
            operation,
            updateButtons);

    private async void AddSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e) => await AddSubscriptionAsync();

    private async void RemoveSubscriptionButton_OnClicked(object? sender, RoutedEventArgs e) => await RemoveSubscriptionAsync();

    protected virtual Task AddSubscriptionAsync() => Task.CompletedTask;

    protected virtual Task RemoveSubscriptionAsync() => Task.CompletedTask;
}

public class WorkRecommendedPage : WorkTypeWorksPage
{
    public WorkRecommendedPage() : this(PixevalSettings.WorkType)
    {
    }

    public WorkRecommendedPage(WorkType workType, IWorkViewViewModel? viewModel = null)
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

    public WorkNewPage(WorkType workType, IWorkViewViewModel? viewModel = null)
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

    public WorkPostsPage(UserBasicInfo user, WorkType workType, IWorkViewViewModel? viewModel = null)
    {
        _user = user;
        EnableAddSubscriptionButton();
        InitializeSource(workType, viewModel);
        if (viewModel is not null)
            UpdateSubscriptionButtons(_user.Id, WorkSubscriptionType.Posts, GetSubscriptionWorkKind(workType));
    }

    protected override IFetchEngine<IWorkEntry> GetFetchEngine(MakoClient makoClient, WorkType workType)
    {
        return makoClient.WorkPosted(workType, _user.Id);
    }

    protected override void OnSourceChanged(IFetchEngine<IWorkEntry> engine, WorkType workType)
    {
        var workKind = GetSubscriptionWorkKind(workType);
        App.AppViewModel.QueueWorkSubscriptionSyncCurrentSource(
            _user.Id,
            WorkSubscriptionType.Posts,
            workKind,
            engine);
        UpdateSubscriptionButtons(_user.Id, WorkSubscriptionType.Posts, workKind);
    }

    protected override async Task AddSubscriptionAsync()
    {
        var workType = WorkTypeComboBox.GetSelectedValue<WorkType>();
        var workKind = GetSubscriptionWorkKind(workType);

        _ = await RunSubscriptionOperationAsync(
            () => Task.FromResult(WorkSubscriptionHelper.TryAddOrUpdateUser(_user, WorkSubscriptionType.Posts, workKind)),
            () => UpdateSubscriptionButtons(WorkTypeComboBox.GetSelectedValue<WorkType>()));
    }

    protected override async Task RemoveSubscriptionAsync()
    {
        _ = await RunSubscriptionOperationAsync(
            RemoveCurrentSubscriptionAsync,
            () => UpdateSubscriptionButtons(WorkTypeComboBox.GetSelectedValue<WorkType>()));
    }

    private async Task<bool> RemoveCurrentSubscriptionAsync()
    {
        if (GetCurrentSubscriptionId() is not { } historyEntryId)
            return false;

        _ = await SubscriptionService.TryRemoveAsync(historyEntryId);
        return true;
    }

    private int? GetCurrentSubscriptionId() =>
        SubscriptionService.TryGetSubscription(
            _user.Id,
            WorkSubscriptionType.Posts,
            GetSubscriptionWorkKind(WorkTypeComboBox.GetSelectedValue<WorkType>()))?.HistoryEntryId;

    private void UpdateSubscriptionButtons(WorkType workType) =>
        UpdateSubscriptionButtons(_user.Id, WorkSubscriptionType.Posts, GetSubscriptionWorkKind(workType));

    private static WorkSubscriptionWorkKind GetSubscriptionWorkKind(WorkType workType) => workType switch
    {
        WorkType.Illustration => WorkSubscriptionWorkKind.Illustration,
        WorkType.Manga => WorkSubscriptionWorkKind.Manga,
        WorkType.Novel => WorkSubscriptionWorkKind.Novel,
        _ => throw new ArgumentOutOfRangeException(nameof(workType))
    };
}
