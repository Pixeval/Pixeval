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
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Settings;
using Pixeval.Utilities;
using Pixeval.Views.Settings;
using Pixeval.Views.Viewers;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class NovelViewerPageViewModel : PagedViewerViewModel, IDisposable
{
    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    [ObservableProperty]
    public partial IReadOnlyList<Page>? PanePages { get; private set; }

    public NovelViewerPageViewModel(IEnumerable<NovelItemViewModel> novelViewModels, int currentNovelIndex)
    {
        NovelsSource = [.. novelViewModels];
        CurrentWorkIndex = currentNovelIndex;
    }

    public NovelViewerPageViewModel(NovelViewViewModel viewModel, int currentNovelIndex)
    {
        ViewModelSource = new NovelViewViewModel(viewModel);
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentWorkIndex = Novels.IndexOf(CurrentNovel);
        CurrentWorkIndex = currentNovelIndex;
    }

    private NovelViewViewModel? ViewModelSource { get; }

    public NovelItemViewModel[]? NovelsSource { get; }

    public IList<NovelItemViewModel> Novels => ViewModelSource?.DataProvider.View ?? (IList<NovelItemViewModel>) NovelsSource!;

    public NovelItemViewModel CurrentNovel => Novels[CurrentWorkIndex];

    public long NovelId => CurrentNovel.Entry.Id;

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
            BuildPanePages();
            _ = LoadCurrentNovelAsync();

            OnPropertyChanged();
            OnPropertyChanged(nameof(NovelId));
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
    public override int WorkCount => Novels.Count;

    public bool IsMultiPage => PageCount > 1;

    public string CurrentMarkdown => PageCount is 0 ? string.Empty : _pageMarkdowns[CurrentPageIndex];

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
                        .Enum(t => t.NovelFontWeight, FontWeightExtension.Items, t => t.PropertyChanged += (_, _) => OnPropertyChanged(nameof(NovelFontWeight)))
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

    public FontFamily NovelFontFamilyObject => new(Settings.NovelFontFamily);

    public FontWeight NovelFontWeight => Settings.NovelFontWeight;

    public int NovelFontSize => Settings.NovelFontSize;

    public int NovelLineHeight => Settings.NovelLineHeight;

    public int NovelMaxWidth => Settings.NovelMaxWidth;

    #endregion

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _loadingCts.Cancel();
        ViewModelSource?.Dispose();
    }

    ~NovelViewerPageViewModel() => Dispose();

    private List<string> _pageMarkdowns = [];

    private CancellationTokenSource _loadingCts = new();

    private async Task LoadCurrentNovelAsync()
    {
        await _loadingCts.CancelAsync();
        _loadingCts = new CancellationTokenSource();
        var token = _loadingCts.Token;

        IsLoading = true;
        try
        {
            var content = await CurrentNovel.ContentAsync;
            token.ThrowIfCancellationRequested();

            BrowseHistoryPersistentManager.AddHistory(CurrentNovel.Entry);
            var markdowns = await Task.Run(() => BuildPageMarkdowns(content), token);
            token.ThrowIfCancellationRequested();

            _pageMarkdowns = markdowns;
            CurrentPageIndex = 0;
            OnPropertyChanged(nameof(PageCount));
            OnPropertyChanged(nameof(IsMultiPage));
        }
        catch (OperationCanceledException)
        {
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

            if (pages.Count == 0)
                pages.Add(string.Empty);

            return pages;
        }
    }

    private void BuildPanePages()
    {
        PanePages =
        [
            new WorkInfoPage(CurrentNovel.Entry),
            new CommentsPage(new CommentsViewViewModel(SimpleWorkType.Novel, NovelId)),
            SettingsPage
        ];
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
}
