using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Database.Managers;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO.Caching;
using WinUI3Utilities;

namespace Pixeval.Controls;

/// <summary>
/// 比<see cref="NovelContext"/>多了关于RTF渲染的内容
/// </summary>
public partial class DocumentViewerViewModel(FrameworkElement frameworkElement) : UiObservableObject(frameworkElement), INovelContext<ImageSource>, INotifyPropertyChanged
{
    /// <summary>
    /// 当前页码，加载完之前为-1
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentParagraphs))]
    public partial int CurrentPage { get; set; } = -1;

    /// <summary>
    /// 总页数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMultiPage))]
    public partial int PageCount { get; private set; }

    /// <summary>
    /// 是否多页
    /// </summary>
    public bool IsMultiPage => PageCount > 1;

    /// <summary>
    /// 是否正在加载
    /// </summary>
    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    /// <summary>
    /// 是否加载成功
    /// </summary>
    [ObservableProperty]
    [MemberNotNullWhen(true, nameof(NovelContent))]
    public partial bool LoadSuccessfully { get; private set; } = false;

    /// <summary>
    /// 翻译后的文本
    /// </summary>
    /// <remarks>为null不触发改变</remarks>
    [ObservableProperty]
    public partial string TranslatedText { get; private set; } = "";

    /// <summary>
    /// <see langword="true"/>能用，<see langword="false"/>不能用
    /// </summary>
    private bool ExtensionRunningLock
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            foreach (var command in ExtensionCommands)
                command.NotifyCanExecuteChanged();
        }
    } = true;

    public List<XamlUICommand> ExtensionCommands { get; } = [];

    public List<List<Paragraph>> Pages { get; } = [];

    public List<Paragraph>? CurrentParagraphs => -1 < CurrentPage && CurrentPage < PageCount ? Pages[CurrentPage] : null;

    public async Task LoadAsync(NovelItemViewModel novelItem)
    {
        if (LoadSuccessfully || IsLoading)
            return;
        IsLoading = true;
        try
        {
            NovelContent = await novelItem.GetNovelContentAsync();
            ((INovelContext<ImageSource>) this).InitImages();
            LoadRtfContent();
            if (CurrentPage is 0)
            {
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(CurrentParagraphs));
            }
            else
                CurrentPage = 0;
            _ = LoadImagesAsync();
            BrowseHistoryPersistentManager.AddHistory(novelItem.Entry);
            LoadSuccessfully = true;
        }
        finally
        {
            IsLoading = false;
        }

        return;

        void LoadRtfContent()
        {
            var index = 0;
            var length = NovelContent.Text.Length;
            var parser = new PixivNovelRtfParser();
            Pages.Add(parser.Parse(NovelContent.Text, ref index, this));
            while (index < length)
            {
                if (LoadingCancellationTokenSource.IsCancellationRequested)
                    break;
                Pages.Add(parser.Parse(NovelContent.Text, ref index, this));
            }
            PageCount = Pages.Count;
        }

        async Task LoadImagesAsync()
        {
            foreach (var illust in NovelContent.Illusts)
            {
                if (LoadingCancellationTokenSource.IsCancellationRequested)
                    break;
                var key = (illust.Id, illust.Page);
                IllustrationImages[key] = await CacheHelper.GetSourceFromCacheAsync(illust.ThumbnailUrl, cancellationToken: LoadingCancellationTokenSource.Token);
                OnPropertyChanged(nameof(IllustrationImages) + key.GetHashCode());
            }

            foreach (var image in NovelContent.Images)
            {
                if (LoadingCancellationTokenSource.IsCancellationRequested)
                    break;
                UploadedImages[image.NovelImageId] = await CacheHelper.GetSourceFromCacheAsync(image.ThumbnailUrl, cancellationToken: LoadingCancellationTokenSource.Token);
                OnPropertyChanged(nameof(UploadedImages) + image.NovelImageId);
            }
        }
    }

    public ICommand GetTransformExtensionCommand(ITextTransformerCommandExtension extension)
    {
        var command = new XamlUICommand();
        command.CanExecuteRequested += ExtensionCanExecuteRequested;
        command.ExecuteRequested += OnCommandOnExecuteRequested;
        ExtensionCommands.Add(command);
        return command;

        void ExtensionCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = LoadSuccessfully && ExtensionRunningLock;
        }


        async void OnCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (!LoadSuccessfully)
                return;

            if (!string.IsNullOrEmpty(TranslatedText))
            {
                TranslatedText = "";
                return;
            }

            ExtensionRunningLock = false;
            try
            {
                var transformer = args.Parameter.To<ITextTransformerCommandExtension>();
                var token = LoadingCancellationTokenSource.Token;
                if (token.IsCancellationRequested)
                    return;
                FrameworkElement.InfoGrowl(NovelViewerPageResources.ApplyingTransformerExtensions);
                // 运行扩展
                var result = await transformer.TransformAsync(CurrentMdPage, TextTransformerType.Novel);
                if (result is null)
                {
                    FrameworkElement.ErrorGrowl(NovelViewerPageResources.TransformerExtensionFailed);
                    return;
                }
                // 显示结果
                TranslatedText = result;
                if (!token.IsCancellationRequested)
                    FrameworkElement.SuccessGrowl(NovelViewerPageResources.TransformerExtensionFinishedSuccessfully);
            }
            finally
            {
                ExtensionRunningLock = true;
            }
        }
    }

    public string CurrentMdPage
    {
        get
        {
            var currentPageCount = _markdownTexts.Count;
            if (currentPageCount > CurrentPage)
                return _markdownTexts[CurrentPage];

            var length = NovelContent.Text.Length;
            for (var i = currentPageCount; _lastIndex < length || CurrentPage >= i; ++i)
            {
                var sb = new StringBuilder();
                var parser = new PixivNovelMdDisplayParser(sb, i);
                _ = parser.Parse(NovelContent.Text, ref _lastIndex, this);
                _markdownTexts.Add(_lastIndex >= length
                    ? sb.ToString()
                    // \r\n---\r\n
                    : sb.ToString(0, sb.Length - 7));
            }

            return _markdownTexts.Count > CurrentPage ? _markdownTexts[CurrentPage] : "";
        }
    }

    private int _lastIndex;

    private readonly List<string> _markdownTexts = [];

    [ObservableProperty]
    public partial NovelContent NovelContent { get; private set; } = null!;

    public Dictionary<(long, int), NovelIllustInfo> IllustrationLookup { get; } = [];

    public Dictionary<(long, int), ImageSource> IllustrationImages { get; } = [];

    public Dictionary<long, ImageSource> UploadedImages { get; } = [];

    public CancellationTokenSource LoadingCancellationTokenSource { get; } = new();

    public string? ImageExtension => null;
}
