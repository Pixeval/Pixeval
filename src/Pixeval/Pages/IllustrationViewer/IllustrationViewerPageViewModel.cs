#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewerPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.System.UserProfile;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.IllustrationView;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.IllustrationViewer;

public partial class IllustrationViewerPageViewModel : DetailedObservableObject, IDisposable
{
    public Window Window { get; set; } = null!;

    public FrameworkElement WindowContent => Window.Content.To<FrameworkElement>();

    private const ThumbnailUrlOption Option = ThumbnailUrlOption.SquareMedium;

    private bool _isFullScreen;

    [ObservableProperty]
    private bool _isInfoPaneOpen;

    // The reason why we don't put UserProfileImageSource into IllustrationViewModel
    // is because the whole array of Illustrations is just representing the same 
    // illustration's different manga pages, so all of them have the same illustrator
    // If the UserProfileImageSource is in IllustrationViewModel and the illustration
    // itself is a manga then all of the IllustrationViewModel in Illustrations will
    // request the same user profile image which is pointless and will (inevitably) causing
    // the waste of system resource
    [ObservableProperty]
    private ImageSource? _userProfileImageSource;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="illustrationViewModels"></param>
    /// <param name="currentIllustrationIndex"></param>
    public IllustrationViewerPageViewModel(IEnumerable<IllustrationItemViewModel> illustrationViewModels, int currentIllustrationIndex)
    {
        IllustrationsSource = illustrationViewModels.ToArray();
        IllustrationInfoTag.Parameter = this;
        // ViewModel.DataProvider.View.CurrentItem为null，而且只设置这个属性会导致空引用
        CurrentIllustrationIndex = currentIllustrationIndex;

        InitializeCommands();
    }

    /// <summary>
    /// 当拥有DataProvider的时候调用这个构造函数，dispose的时候会自动dispose掉DataProvider
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentIllustrationIndex"></param>
    /// <remarks>
    /// illustrations should contains only one item if the illustration is a single
    /// otherwise it contains the entire manga data
    /// </remarks>
    public IllustrationViewerPageViewModel(IllustrationViewViewModel viewModel, int currentIllustrationIndex)
    {
        ViewModelSource = new IllustrationViewViewModel(viewModel);
        IllustrationInfoTag.Parameter = this;
        ViewModelSource.DataProvider.View.FilterChanged += (_, _) => CurrentIllustrationIndex = Illustrations.IndexOf(CurrentIllustration);
        // ViewModel.DataProvider.View.CurrentItem为null，而且只设置这个属性会导致空引用
        CurrentIllustrationIndex = currentIllustrationIndex;

        InitializeCommands();
    }

    public bool IsFullScreen
    {
        get => _isFullScreen;
        set
        {
            if (value == _isFullScreen)
                return;
            _isFullScreen = value;
            if (value)
            {
                FullScreenCommand.Description = IllustrationViewerPageResources.BackToWindow;
                FullScreenCommand.IconSource = new SymbolIconSource { Symbol = Symbol.BackToWindow };
            }
            else
            {
                FullScreenCommand.Description = IllustrationViewerPageResources.FullScreen;
                FullScreenCommand.IconSource = new SymbolIconSource { Symbol = Symbol.FullScreen };
            }
            OnPropertyChanged();
        }
    }

    private IllustrationViewViewModel? ViewModelSource { get; }

    public IllustrationItemViewModel[]? IllustrationsSource { get; }

    public long IllustrationId => CurrentIllustration.Illustrate.Id;

    public UserInfo Illustrator => CurrentIllustration.Illustrate.User;

    public string IllustratorName => Illustrator.Name;

    public long IllustratorUid => Illustrator.Id;

    public bool IsManga => _pages.Length > 1;

    public bool IsUgoira => CurrentIllustration.IsUgoira;

    public void Dispose()
    {
        foreach (var illustrationViewModel in Illustrations)
            illustrationViewModel.UnloadThumbnail(this, Option);
        _pages?.ForEach(i => i.Dispose());
        (UserProfileImageSource as SoftwareBitmapSource)?.Dispose();
        ViewModelSource?.Dispose();
    }

    public ImageViewerPageViewModel NextPage()
    {
        ++CurrentPageIndex;
        return CurrentImage;
    }

    public ImageViewerPageViewModel PrevPage()
    {
        --CurrentPageIndex;
        return CurrentImage;
    }

    public Task LoadMoreAsync(uint count) => ViewModelSource?.LoadMoreAsync(count) ?? Task.CompletedTask;

    #region Tags for IllustrationInfoAndCommentsNavigationView

    public NavigationViewTag IllustrationInfoTag = new(typeof(IllustrationInfoPage), null);
    public NavigationViewTag CommentsTag = new(typeof(CommentsPage), null);
    public NavigationViewTag RelatedWorksTag = new(typeof(RelatedWorksPage), null);

    #endregion

    #region Current相关

    /// <summary>
    /// 插画列表
    /// </summary>
    public IList<IllustrationItemViewModel> Illustrations => ViewModelSource?.DataProvider.View ?? (IList<IllustrationItemViewModel>)IllustrationsSource!;

    /// <summary>
    /// 当前插画
    /// </summary>
    public IllustrationItemViewModel CurrentIllustration => Illustrations[CurrentIllustrationIndex];

    /// <summary>
    /// 当前插画的页面
    /// </summary>
    public IllustrationItemViewModel CurrentPage => _pages[CurrentPageIndex];

    /// <summary>
    /// 当前插画的索引
    /// </summary>
    public int CurrentIllustrationIndex
    {
        get => _currentIllustrationIndex;
        set
        {
            if (value is -1)
                return;

            var oldValue = _currentIllustrationIndex;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            var oldTag = _pages?[CurrentPageIndex].Id ?? 0;

            _currentIllustrationIndex = value;
            _pages?.ForEach(i => i.Dispose());
            _pages = CurrentIllustration.GetMangaIllustrationViewModels().ToArray();
            // 保证_pages里所有的IllustrationViewModel都是生成的，从而删除的时候一律DisposeForce

            RelatedWorksTag.Parameter = IllustrationId;
            // IllustrationInfoTag.Parameter = this;
            CommentsTag.Parameter = (App.AppViewModel.MakoClient.IllustrationComments(IllustrationId).Where(c => c is not null), IllustrationId);

            LoadUserProfile().Discard();

            CurrentPageIndex = 0;
            // 更新PlayGif按钮的状态
            OnPropertyChanged(nameof(IsUgoira));
            PlayGifCommand.Description = PlayGifCommand.Label = IllustrationViewerPageResources.PauseGif;
            PlayGifCommand.IconSource = new SymbolIconSource { Symbol = Symbol.Stop };
            OnDetailedPropertyChanged(oldValue, value, oldTag, CurrentPage.Id);
            return;

            async Task LoadUserProfile()
            {
                if (Illustrator is { ProfileImageUrls.Medium: { } profileImage })
                {
                    var result = await App.AppViewModel.MakoClient.DownloadSoftwareBitmapSourceAsync(profileImage);
                    UserProfileImageSource = result is Result<SoftwareBitmapSource>.Success { Value: var avatar }
                            ? avatar
                            : await AppContext.GetPixivNoProfileImageAsync();
                }
            }
        }
    }

    /// <summary>
    /// 当前插画的页面索引
    /// </summary>
    public int CurrentPageIndex
    {
        get => _currentPageIndex;
        set
        {
            _currentPageIndex = value;
            OnPropertyChanged(nameof(NextButtonEnable));
            OnPropertyChanged(nameof(PrevButtonEnable));
            CurrentImage = new ImageViewerPageViewModel(this, CurrentPage);
        }
    }

    /// <inheritdoc cref="CurrentIllustrationIndex"/>
    private int _currentIllustrationIndex;

    /// <inheritdoc cref="CurrentPageIndex"/>
    private int _currentPageIndex;

    /// <summary>
    /// 一个插画所有的页面
    /// </summary>
    private IllustrationItemViewModel[] _pages = null!;

    /// <summary>
    /// 当前图片的ViewModel
    /// </summary>
    [ObservableProperty]
    private ImageViewerPageViewModel _currentImage = null!;

    #endregion

    #region Helper Functions

    public Visibility NextButtonEnable => NextButtonAction is null ? Visibility.Collapsed : Visibility.Visible;

    public bool NextIllustrationEnable => Illustrations.Count > CurrentIllustrationIndex + 1;

    /// <summary>
    /// <see langword="true"/>: next page<br/>
    /// <see langword="false"/>: next illustration<br/>
    /// <see langword="null"/>: none
    /// </summary>
    public bool? NextButtonAction
    {
        get
        {
            if (CurrentPageIndex < _pages.Length - 1)
            {
                return true;
            }

            if (NextIllustrationEnable)
            {
                return false;
            }

            return null;
        }
    }

    public Visibility PrevButtonEnable => PrevButtonAction is null ? Visibility.Collapsed : Visibility.Visible;

    public bool PrevIllustrationEnable => CurrentIllustrationIndex > 0;

    /// <summary>
    /// <see langword="true"/>: prev page<br/>
    /// <see langword="false"/>: prev illustration<br/>
    /// <see langword="null"/>: none
    /// </summary>
    public bool? PrevButtonAction
    {
        get
        {
            if (CurrentPageIndex > 0)
            {
                return true;
            }

            if (PrevIllustrationEnable)
            {
                return false;
            }

            return null;
        }
    }

    #endregion

    #region Commands

    public void UpdateCommandCanExecute()
    {
        PlayGifCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        RestoreResolutionCommand.NotifyCanExecuteChanged();
        ZoomInCommand.NotifyCanExecuteChanged();
        ZoomOutCommand.NotifyCanExecuteChanged();
        RotateClockwiseCommand.NotifyCanExecuteChanged();
        RotateCounterclockwiseCommand.NotifyCanExecuteChanged();
        MirrorCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
        ShareCommand.NotifyCanExecuteChanged();
    }

    private void InitializeCommands()
    {
        RestoreResolutionCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;

        CopyCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        CopyCommand.ExecuteRequested += async (_, _) =>
        {
            var teachingTip = WindowContent.CreateTeachingTip();

            var progress = null as Progress<int>;
            if (CurrentImage.IllustrationViewModel.IsUgoira)
                progress = new(d => teachingTip.Show(IllustrationViewerPageResources.UgoiraProcessing.Format(d), TeachingTipSeverity.Processing, isLightDismissEnabled: true));
            else
                teachingTip.Show(IllustrationViewerPageResources.ImageProcessing, TeachingTipSeverity.Processing, isLightDismissEnabled: true);
            if (await CurrentImage.GetOriginalImageSourceForClipBoardAsync(progress) is { } source)
            {
                UiHelper.ClipboardSetBitmap(source);
                teachingTip.ShowAndHide(IllustrationViewerPageResources.ImageSetToClipBoard);
            }
        };

        PlayGifCommand.CanExecuteRequested += (_, e) => e.CanExecute = IsUgoira && CurrentImage.LoadSuccessfully;
        PlayGifCommand.ExecuteRequested += PlayGifCommandOnExecuteRequested;

        // 相当于鼠标滚轮滚动10次，方便快速缩放
        ZoomOutCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ZoomOutCommand.ExecuteRequested += (_, _) => CurrentImage.Zoom(-1200);

        ZoomInCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ZoomInCommand.ExecuteRequested += (_, _) => CurrentImage.Zoom(1200);

        RotateClockwiseCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        RotateClockwiseCommand.ExecuteRequested += (_, _) => CurrentImage.RotationDegree += 90;

        RotateCounterclockwiseCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        RotateCounterclockwiseCommand.ExecuteRequested += (_, _) => CurrentImage.RotationDegree -= 90;

        MirrorCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        MirrorCommand.ExecuteRequested += (_, _) => CurrentImage.IsMirrored = !CurrentImage.IsMirrored;

        SaveCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        SaveCommand.ExecuteRequested += SaveAsync;

        SaveAsCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        SaveAsCommand.ExecuteRequested += SaveAsAsync;

        ShareCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ShareCommand.ExecuteRequested += ShareCommandExecuteRequested;

        SetAsLockScreenCommand.CanExecuteRequested += IsNotUgoiraAndLoadingCompletedCanExecuteRequested;
        SetAsLockScreenCommand.ExecuteRequested += SetAsLockScreenCommandOnExecuteRequested;

        SetAsBackgroundCommand.CanExecuteRequested += IsNotUgoiraAndLoadingCompletedCanExecuteRequested;
        SetAsBackgroundCommand.ExecuteRequested += SetAsBackgroundCommandOnExecuteRequested;

        FullScreenCommand.ExecuteRequested += (_, _) => IsFullScreen = !IsFullScreen;

        RestoreResolutionCommand.ExecuteRequested += FlipRestoreResolutionCommand;
    }

    private void LoadingCompletedCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = CurrentImage.LoadSuccessfully;
    }

    private void IsNotUgoiraAndLoadingCompletedCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = !IsUgoira && CurrentImage.LoadSuccessfully;
    }

    private async void SetAsBackgroundCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (CurrentImage.OriginalImageStream is null)
        {
            WindowContent.ShowTeachingTipAndHide(IllustrationViewerPageResources.OriginalmageStreamIsEmptyContent, TeachingTipSeverity.Error);
            return;
        }

        var guid = await CurrentImage.OriginalImageStream.Sha1Async();
        if (await AppKnownFolders.SavedWallPaper.TryGetFileRelativeToSelfAsync(guid) is null)
        {
            var path = Path.Combine(AppKnownFolders.SavedWallPaper.Self.Path, guid);
            using var scope = App.AppViewModel.AppServicesScope;
            var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
            var intrinsicTask = await factory.TryCreateIntrinsicAsync(CurrentIllustration, CurrentImage.OriginalImageStream!, path);
            App.AppViewModel.DownloadManager.QueueTask(intrinsicTask);
            await intrinsicTask.Completion.Task;
        }

        _ = await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(await AppKnownFolders.SavedWallPaper.GetFileAsync(guid));
        ToastNotificationHelper.ShowTextToastNotification(
            IllustrationViewerPageResources.SetAsSucceededTitle,
            IllustrationViewerPageResources.SetAsBackgroundSucceededTitle,
            AppContext.AppLogoNoCaptionUri);
    }

    private async void SetAsLockScreenCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (CurrentImage.OriginalImageStream is null)
        {
            WindowContent.ShowTeachingTipAndHide(IllustrationViewerPageResources.OriginalmageStreamIsEmptyContent, TeachingTipSeverity.Error);
            return;
        }

        var guid = await CurrentImage.OriginalImageStream.Sha1Async();
        if (await AppKnownFolders.SavedWallPaper.TryGetFileRelativeToSelfAsync(guid) is null)
        {
            var path = Path.Combine(AppKnownFolders.SavedWallPaper.Self.Path, guid);
            using var scope = App.AppViewModel.AppServicesScope;
            var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
            var intrinsicTask = await factory.TryCreateIntrinsicAsync(CurrentIllustration, CurrentImage.OriginalImageStream!, path);
            App.AppViewModel.DownloadManager.QueueTask(intrinsicTask);
            await intrinsicTask.Completion.Task;
        }

        _ = await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(await AppKnownFolders.SavedWallPaper.GetFileAsync(guid));
        ToastNotificationHelper.ShowTextToastNotification(
            IllustrationViewerPageResources.SetAsSucceededTitle,
            IllustrationViewerPageResources.SetAsBackgroundSucceededTitle,
            AppContext.AppLogoNoCaptionUri);
    }

    private async void SaveAsync(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        await CurrentPage.SaveAsync(CurrentImage.OriginalImageStream);
        WindowContent.ShowTeachingTipAndHide("已保存");
    }

    private async void SaveAsAsync(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (await CurrentIllustration.SaveAsAsync(Window))
            WindowContent.ShowTeachingTipAndHide("已保存");
        else
            WindowContent.ShowTeachingTipAndHide("已取消另存为操作", TeachingTipSeverity.Information);
    }

    private void ShareCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (CurrentImage.LoadSuccessfully)
            Window.ShowShareUi();
        else
            WindowContent.ShowTeachingTipAndHide(IllustrationViewerPageResources.CannotShareImageForNowTitle, TeachingTipSeverity.Warning, IllustrationViewerPageResources.CannotShareImageForNowContent);
    }

    private void PlayGifCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        CurrentImage.IsPlaying = !CurrentImage.IsPlaying;
        if (CurrentImage.IsPlaying)
        {
            PlayGifCommand.Description = PlayGifCommand.Label = IllustrationViewerPageResources.PauseGif;
            PlayGifCommand.IconSource = new SymbolIconSource { Symbol = Symbol.Stop };
        }
        else
        {
            PlayGifCommand.Description = PlayGifCommand.Label = IllustrationViewerPageResources.PlayGif;
            PlayGifCommand.IconSource = new SymbolIconSource { Symbol = Symbol.Play };
        }
    }

    public void FlipRestoreResolutionCommand(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (CurrentImage.IsFit)
        {
            RestoreResolutionCommand.Label = IllustrationViewerPageResources.UniformToFillResolution;
            RestoreResolutionCommand.IconSource = FontIconSymbols.FitPageE9A6.GetFontIconSource();
            CurrentImage.ShowMode = ZoomableImageMode.Original;
        }
        else
        {
            RestoreResolutionCommand.Label = IllustrationViewerPageResources.RestoreOriginalResolution;
            RestoreResolutionCommand.IconSource = FontIconSymbols.WebcamE8B8.GetFontIconSource();
            CurrentImage.ShowMode = ZoomableImageMode.Fit;
        }
    }

    public XamlUICommand IllustrationInfoAndCommentsCommand { get; } =
        IllustrationViewerPageResources.IllustrationInfoAndComments.GetCommand(FontIconSymbols.InfoE946, VirtualKey.F12);

    public XamlUICommand CopyCommand { get; } = IllustrationViewerPageResources.Copy.GetCommand(
            FontIconSymbols.CopyE8C8, VirtualKeyModifiers.Control, VirtualKey.C);

    // The gif will be played as soon as its loaded, so the default state is playing and thus we need the button to be pause
    public XamlUICommand PlayGifCommand { get; } = IllustrationViewerPageResources.PauseGif.GetCommand(FontIconSymbols.StopE71A);

    public XamlUICommand ZoomOutCommand { get; } = IllustrationViewerPageResources.ZoomOut.GetCommand(
        FontIconSymbols.ZoomOutE71F, VirtualKey.Subtract);

    public XamlUICommand ZoomInCommand { get; } = IllustrationViewerPageResources.ZoomIn.GetCommand(
        FontIconSymbols.ZoomInE8A3, VirtualKey.Add);

    public XamlUICommand RotateClockwiseCommand { get; } = IllustrationViewerPageResources.RotateClockwise.GetCommand(
        FontIconSymbols.RotateE7AD, VirtualKeyModifiers.Control, VirtualKey.R);

    public XamlUICommand RotateCounterclockwiseCommand { get; } =
        IllustrationViewerPageResources.RotateCounterclockwise.GetCommand(
            null!, VirtualKeyModifiers.Control, VirtualKey.L);

    public XamlUICommand MirrorCommand { get; } =
        IllustrationViewerPageResources.Mirror.GetCommand(
            FontIconSymbols.CollatePortraitF57C, VirtualKeyModifiers.Control, VirtualKey.M);

    public XamlUICommand SaveCommand { get; } = IllustrationViewerPageResources.Save.GetCommand(
        FontIconSymbols.SaveE74E, VirtualKeyModifiers.Control, VirtualKey.S);

    public XamlUICommand SaveAsCommand { get; } = IllustrationViewerPageResources.SaveAs.GetCommand(
        FontIconSymbols.SaveAsE792, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.S);

    public XamlUICommand SetAsCommand { get; } = IllustrationViewerPageResources.SetAs.GetCommand(FontIconSymbols.PersonalizeE771);

    public StandardUICommand ShareCommand { get; } = new(StandardUICommandKind.Share);

    public XamlUICommand SetAsLockScreenCommand { get; } = new() { Label = IllustrationViewerPageResources.LockScreen };

    public XamlUICommand SetAsBackgroundCommand { get; } = new() { Label = IllustrationViewerPageResources.Background };

    public XamlUICommand FullScreenCommand { get; } = IllustrationViewerPageResources.FullScreen.GetCommand(FontIconSymbols.FullScreenE740);

    public XamlUICommand RestoreResolutionCommand { get; } = IllustrationViewerPageResources.RestoreOriginalResolution.GetCommand(FontIconSymbols.WebcamE8B8);

    #endregion
}
