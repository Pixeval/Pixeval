// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoSettingsPage;
using AutoSettingsPage.Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Settings;
using Pixeval.Views.Settings;
using Pixeval.Views.Viewers;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class NovelViewerPageViewModel : PagedViewerViewModel, IDisposable
{
    private readonly Dictionary<int, NovelItemViewModel> _refreshedNovels = [];

    private readonly bool _needRefresh;

    private readonly ISourceView<NovelItemViewModel>? _sourceView;

    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    [ObservableProperty]
    public partial string? LoadErrorMessage { get; private set; }

    public IReadOnlyList<Page> PanePages => CurrentNovel is { } currentNovel
        ?
        [
            new WorkInfoPage(currentNovel.Entry),
            new CommentsPage(new CommentsViewViewModel(SimpleWorkType.Novel, currentNovel.Entry.Id)),
            SettingsPage
        ]
        : [];

    public NovelViewerPageViewModel(NovelItemViewModel novelViewModel, bool needRefresh)
    {
        _needRefresh = needRefresh;
        CurrentNovel = novelViewModel;
        CurrentWorkIndex = 0;
    }

    public NovelViewerPageViewModel(long id)
    {
        _ = LoadSingleNovelAsync(id, _loadingCts.Token);
    }

    public NovelViewerPageViewModel(ISourceView<NovelItemViewModel> dataProvider, int currentNovelIndex, bool needRefresh)
    {
        _needRefresh = needRefresh;
        _sourceView = dataProvider;
        CurrentWorkIndex = currentNovelIndex;
    }

    public IReadOnlyList<NovelItemViewModel>? Novels => _sourceView?.View;

    public NovelItemViewModel? CurrentNovel
    {
        get
        {
            if (_refreshedNovels.TryGetValue(CurrentWorkIndex, out var value))
                return value;

            if (field is not null)
                return field;

            return CurrentWorkIndex < 0 || CurrentWorkIndex >= WorkCount
                ? null
                : _sourceView?.View[CurrentWorkIndex];
        }
        private set
        {
            if (Equals(value, field))
                return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NovelId));
            OnPropertyChanged(nameof(PanePages));
        }
    }

    public long NovelId => CurrentNovel?.Entry.Id ?? 0;

    public override int CurrentWorkIndex
    {
        get;
        set
        {
            if (value is -1)
                return;
            if (value == field)
                return;

            field = value;
            _ = LoadCurrentNovelAsync();

            OnPropertyChanged();
            OnPropertyChanged(nameof(NovelId));
            OnPropertyChanged(nameof(CurrentNovel));
            OnPropertyChanged(nameof(PanePages));
        }
        // 第一次赋值属性时会判断 value == field，如果是0则无法进入set方法体
        // ReSharper disable once MemberInitializerValueIgnored
    } = -1;

    public override int CurrentPageIndex
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentNovel));
            OnPropertyChanged(nameof(CurrentMarkdown));
            OnPropertyChanged(nameof(NextButtonText));
            OnPropertyChanged(nameof(PrevButtonText));
            NextCommand.NotifyCanExecuteChanged();
            PrevCommand.NotifyCanExecuteChanged();
            NextWorkCommand.NotifyCanExecuteChanged();
            PrevWorkCommand.NotifyCanExecuteChanged();
        }
    }

    public override int PageCount => _pageMarkdowns.Count;

    /// <inheritdoc />
    public override int WorkCount => Novels?.Count ?? 1;

    public bool IsMultiPage => PageCount > 1;

    public string CurrentMarkdown => PageCount is 0 ? "" : _pageMarkdowns[CurrentPageIndex];

    #region Settings

    private SettingsSubView SettingsPage
    {
        get
        {
            LocalSettingsEntryHelper.Initialize();
            return field ??= new SettingsSubView(
                SettingsBuilder.CreateGroupList(App.AppViewModel.AppSettings)
                    .NewGroup("")
                    .Config(group => group
                        .Color(t => t.NovelBackground, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelBackgroundBrush)))
                        .Color(t => t.NovelFontColor, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelForegroundBrush)))
                        .Font(t => t.NovelFontFamily, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelFontFamilyObject)))
                        .Enum(t => t.NovelFontWeight, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelFontWeight)))
                        .Int(t => t.NovelFontSize, 5, 100, 1, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelFontSize)))
                        .Int(t => t.NovelLineHeight, 0, 150, 1, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelLineHeight)))
                        .Int(t => t.NovelMaxWidth, 50, 10000, 50, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelMaxWidth))))
                    .Build()[0])
            {
                Header = I18NManager.GetResource(EntryViewerPageResources.NovelSettings)
            };
        }
    }

    private static AppSettings Settings => App.AppViewModel.AppSettings;

    public IBrush NovelBackgroundBrush => new SolidColorBrush(Color.FromUInt32(Settings.NovelBackground));

    public IBrush NovelForegroundBrush => new SolidColorBrush(Color.FromUInt32(Settings.NovelFontColor));

    public FontFamily NovelFontFamilyObject => new(string.Join(',', Settings.NovelFontFamily));

    public FontWeight NovelFontWeight => Settings.NovelFontWeight;

    public int NovelFontSize => Settings.NovelFontSize;

    public int NovelLineHeight => Settings.NovelLineHeight;

    public int NovelMaxWidth => Settings.NovelMaxWidth;

    #endregion

    private List<string> _pageMarkdowns = [];

    private CancellationTokenSource _loadingCts = new();

    private async Task LoadSingleNovelAsync(long id, CancellationToken token)
    {
        var novel = await LoadNovelAsync(id, item => CurrentNovel = item, token);
        if (novel is null || token.IsCancellationRequested)
        {
            if (!token.IsCancellationRequested)
                IsLoading = false;
            return;
        }

        if (CurrentWorkIndex is 0)
            await LoadCurrentNovelAsync();
        else
            CurrentWorkIndex = 0;
    }

    private async Task LoadCurrentNovelAsync()
    {
        if (_disposed)
            return;

        var index = CurrentWorkIndex;
        var token = ResetLoadingToken();

        IsLoading = true;
        LoadErrorMessage = null;
        try
        {
            var currentNovel = await GetCurrentNovelAsync(index, token);
            token.ThrowIfCancellationRequested();
            if (currentNovel is null || index != CurrentWorkIndex || _disposed)
                return;

            var content = await currentNovel.ContentAsync;
            token.ThrowIfCancellationRequested();
            if (index != CurrentWorkIndex || _disposed)
                return;

            App.AppViewModel.HistoryPersistHelper.AddBrowseHistory(currentNovel.Entry);
            var markdowns = await Task.Run(() => BuildPageMarkdowns(content), token);
            token.ThrowIfCancellationRequested();
            if (index != CurrentWorkIndex || _disposed)
                return;

            _pageMarkdowns = markdowns;
            CurrentPageIndex = 0;
            OnPropertyChanged(nameof(PageCount));
            OnPropertyChanged(nameof(IsMultiPage));
            OnPropertyChanged(nameof(CurrentNovel));
            OnPropertyChanged(nameof(NovelId));
            OnPropertyChanged(nameof(PanePages));
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
                LoadErrorMessage = e.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested)
                IsLoading = false;
        }

        return;

        static List<string> BuildPageMarkdowns(NovelContent content)
        {
            var context = new NovelDisplayContext(content);
            ((INovelContext<Bitmap>) context).InitImages();

            var pages = new List<string>();
            var text = content.Text;
            var index = 0;
            var pageIndex = 0;

            while (index < text.Length)
            {
                var sb = new StringBuilder();
                var parser = new PixivNovelMdDisplayParser(sb, pageIndex++);
                _ = parser.Parse(text, ref index, context);
                pages.Add(sb.ToString());
            }

            if (pages.Count is 0)
                pages.Add("");

            return pages;
        }
    }

    private async Task<NovelItemViewModel?> GetCurrentNovelAsync(int index, CancellationToken token)
    {
        if (!_needRefresh)
            return CurrentNovel;

        if (_sourceView is not null && _refreshedNovels.TryGetValue(index, out var cached))
            return cached;

        if (CurrentNovel is not { Entry.Id: var id })
            return null;

        return await LoadNovelAsync(
            id,
            novel =>
            {
                // 单项刷新写回当前项，列表刷新只缓存到当前 Viewer，避免污染原始 SourceView。
                if (_sourceView is null)
                    CurrentNovel = novel;
                else
                    _refreshedNovels[index] = novel;
            },
            token);
    }

    private CancellationToken ResetLoadingToken()
    {
        _loadingCts.Cancel();
        _loadingCts.Dispose();
        _loadingCts = new CancellationTokenSource();
        return _loadingCts.Token;
    }

    private async Task<NovelItemViewModel?> LoadNovelAsync(long id, Action<NovelItemViewModel> onLoaded, CancellationToken token)
    {
        IsLoading = true;
        LoadErrorMessage = null;
        try
        {
            var novel = await App.AppViewModel.MakoClient.GetNovelFromIdAsync(id);
            token.ThrowIfCancellationRequested();
            var viewModel = NovelItemViewModel.CreateInstance(novel);
            onLoaded(viewModel);
            return viewModel;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
                LoadErrorMessage = e.Message;
            return null;
        }
    }

    private sealed class NovelDisplayContext(NovelContent novelContent) : INovelContext<Bitmap>
    {
        public NovelContent NovelContent { get; } = novelContent;

        public Dictionary<(long, int), NovelIllustration> IllustrationLookup { get; } = [];

        public Dictionary<(long, int), Bitmap> IllustrationImages { get; } = [];

        public Dictionary<long, Bitmap> UploadedImages { get; } = [];

        public CancellationTokenSource LoadingCts { get; } = new();

        public string? ImageExtension => null;
    }

    #region Dispose

    private bool _disposed;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed)
            return;

        _disposed = true;
        _loadingCts.Cancel();
        _loadingCts.Dispose();
        _sourceView?.Dispose();
    }

    ~NovelViewerPageViewModel() => Dispose();

    #endregion
}
