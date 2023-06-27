#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationViewerPageViewModel.cs
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.UserProfile;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.UserControls.IllustrationView;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.IllustrationViewer;

public partial class IllustrationViewerPageViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private ImageViewerPageViewModel _current = null!;

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private bool _isInfoPaneOpen;

    [ObservableProperty]
    private IllustrationViewModel? _selectedIllustrationViewModel;

    // The reason why we don't put UserProfileImageSource into IllustrationViewModel
    // is because the whole array of Illustrations is just representing the same 
    // illustration's different manga pages, so all of them have the same illustrator
    // If the UserProfileImageSource is in IllustrationViewModel and the illustration
    // itself is a manga then all of the IllustrationViewModel in Illustrations will
    // request the same user profile image which is pointless and will (inevitably) causing
    // the waste of system resource
    [ObservableProperty]
    private ImageSource? _userProfileImageSource;

    // Preserved for illustrator view use
    [ObservableProperty]
    private UserInfo? _userInfo;

    private bool _isFullScreen;

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

    [ObservableProperty]
    private AdvancedCollectionView? _snapshot;

    private readonly IllustrationViewModel[] _illustrations;

    public bool IsDisposed { get; set; }

    public event EventHandler<double>? ZoomChanged;

    // illustrations should contains only one item if the illustration is a single
    // otherwise it contains the entire manga data
    public IllustrationViewerPageViewModel(RiverFlowIllustrationView illustrationView, params IllustrationViewModel[] illustrations) : this(illustrations)
    {
        IllustrationView = illustrationView;
        ContainerGridViewModel = illustrationView.ViewModel;
        IllustrationViewModelInTheGridView = ContainerGridViewModel.DataProvider.IllustrationsView.Cast<IllustrationViewModel>().First(model => model.Id == Current.IllustrationViewModel.Id);
    }

    public IllustrationViewerPageViewModel(params IllustrationViewModel[] illustrations)
    {
        _illustrations = illustrations;
        ImageViewerPageViewModels = illustrations.Select(i => new ImageViewerPageViewModel(this, i)).ToArray();
        ReassignAndResubscribeZoomingEvent(ImageViewerPageViewModels[CurrentIndex]);
        InitializeCommands();
        _ = LoadUserProfile();
    }

    /// <summary>
    /// Copy a new view model
    /// </summary>
    /// <returns></returns>
    public IllustrationViewerPageViewModel CreateNew()
    {
        return IllustrationView is not null ? new IllustrationViewerPageViewModel(IllustrationView, _illustrations) : new IllustrationViewerPageViewModel(_illustrations);
    }

    /// <summary>
    ///     The view model of the GridView that the <see cref="ImageViewerPageViewModels" /> comes from
    /// </summary>
    public IllustrationViewViewModel? ContainerGridViewModel { get; }

    /// <summary>
    ///     The <see cref="RiverFlowIllustrationView" /> that owns <see cref="ContainerGridViewModel" />
    /// </summary>
    public RiverFlowIllustrationView? IllustrationView { get; }

    /// <summary>
    ///     The <see cref="IllustrationViewModelInTheGridView" /> in <see cref="RiverFlowIllustrationView" /> that corresponds to
    ///     current
    ///     <see cref="IllustrationViewerPageViewModel" />
    /// </summary>
    public IllustrationViewModel? IllustrationViewModelInTheGridView { get; }

    /// <summary>
    ///     The index of current illustration in <see cref="RiverFlowIllustrationView" />
    /// </summary>
    public int? IllustrationIndex => ContainerGridViewModel?.DataProvider.IllustrationsView.IndexOf(IllustrationViewModelInTheGridView);

    public ImageViewerPageViewModel[]? ImageViewerPageViewModels { get; }

    public string IllustrationId => FirstIllustrationViewModel?.Illustration.Id.ToString() ?? string.Empty;

    public string? IllustratorName => FirstIllustrationViewModel?.Illustration.User?.Name;

    public string? IllustratorUid => FirstIllustrationViewModel?.Illustration.User?.Id.ToString();

    public bool IsManga => ImageViewerPageViewModels?.Length > 1;

    public bool IsUgoira => Current.IllustrationViewModel.Illustration.IsUgoira();

    public IllustrationViewModel? FirstIllustrationViewModel => FirstImageViewerPageViewModel?.IllustrationViewModel;

    public ImageViewerPageViewModel? FirstImageViewerPageViewModel => ImageViewerPageViewModels?.FirstOrDefault();

    public void Dispose()
    {
        ImageViewerPageViewModels?.ForEach(i => i.Dispose());
        // The first ImageViewerPageViewModel contains the thumbnail that is used to be displayed in the GridView
        // disposing of it will also empty the corresponding image in GridView.
        ImageViewerPageViewModels?.Skip(1).ForEach(i => i.IllustrationViewModel.Dispose());

        (UserProfileImageSource as SoftwareBitmapSource)?.Dispose();
        UserProfileImageSource = null;
        IsDisposed = true;
    }

    public void UpdateCommandCanExecute()
    {
        PlayGifCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        RestoreResolutionCommand.NotifyCanExecuteChanged();
        ZoomInCommand.NotifyCanExecuteChanged();
        ZoomOutCommand.NotifyCanExecuteChanged();
        FullScreenCommand.NotifyCanExecuteChanged();
    }

    public void FlipRestoreResolutionCommand(bool scaled)
    {
        RestoreResolutionCommand.Label = scaled ? IllustrationViewerPageResources.UniformToFillResolution : IllustrationViewerPageResources.RestoreOriginalResolution;
        RestoreResolutionCommand.IconSource = scaled ? FontIconSymbols.Webcam2E960.GetFontIconSource() : FontIconSymbols.WebcamE8B8.GetFontIconSource();
    }

    private void InitializeCommands()
    {
        BookmarkCommand =
            (FirstIllustrationViewModel!.IsBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark)
            .GetCommand(
                MakoHelper.GetBookmarkButtonIconSource(FirstIllustrationViewModel.IsBookmarked),
                VirtualKeyModifiers.Control, VirtualKey.D);

        RestoreResolutionCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;

        BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

        CopyCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;

        PlayGifCommand.CanExecuteRequested += PlayGifCommandOnCanExecuteRequested;
        PlayGifCommand.ExecuteRequested += PlayGifCommandOnExecuteRequested;

        ZoomOutCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ZoomOutCommand.ExecuteRequested += (_, _) => Current.Zoom(-0.5f);

        ZoomInCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ZoomInCommand.ExecuteRequested += (_, _) => Current.Zoom(0.5f);

        SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;
        SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;

        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;
        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;
        ShareCommand.ExecuteRequested += ShareCommandOnExecuteRequested;

        SetAsLockScreenCommand.CanExecuteRequested += SetAsLockScreenCommandOnCanExecuteRequested;
        SetAsLockScreenCommand.ExecuteRequested += SetAsLockScreenCommandOnExecuteRequested;

        SetAsBackgroundCommand.CanExecuteRequested += SetAsBackgroundCommandOnCanExecuteRequested;
        SetAsBackgroundCommand.ExecuteRequested += SetAsBackgroundCommandOnExecuteRequested;
    }

    private void LoadingCompletedCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = Current.LoadingCompletedSuccessfully;
    }

    private async void SetAsBackgroundCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (Current.OriginalImageStream is null)
        {
            ShowAndHide(IllustrationViewerPageResources.OriginalmageStreamIsEmptyContent, Severity.Error);
            return;
        }

        var guid = await Current.OriginalImageStream.Sha1Async();
        if (await AppKnownFolders.SavedWallPaper.TryGetFileRelativeToSelfAsync(guid) is null)
        {
            var path = Path.Combine(AppKnownFolders.SavedWallPaper.Self.Path, guid);
            using var scope = App.AppViewModel.AppServicesScope;
            var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>>();
            var intrinsicTask = await factory.TryCreateIntrinsicAsync(Current.IllustrationViewModel, Current.OriginalImageStream!, path);
            App.AppViewModel.DownloadManager.QueueTask(intrinsicTask);
            await intrinsicTask.Completion.Task;
        }

        await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(await AppKnownFolders.SavedWallPaper.GetFileAsync(guid));
        ToastNotificationHelper.ShowTextToastNotification(
            IllustrationViewerPageResources.SetAsSucceededTitle,
            IllustrationViewerPageResources.SetAsBackgroundSucceededTitle,
            AppContext.AppLogoNoCaptionUri);
    }

    private async void SetAsLockScreenCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (Current.OriginalImageStream is null)
        {
            ShowAndHide(IllustrationViewerPageResources.OriginalmageStreamIsEmptyContent, Severity.Error);
            return;
        }

        var guid = await Current.OriginalImageStream.Sha1Async();
        if (await AppKnownFolders.SavedWallPaper.TryGetFileRelativeToSelfAsync(guid) is null)
        {
            var path = Path.Combine(AppKnownFolders.SavedWallPaper.Self.Path, guid);
            using var scope = App.AppViewModel.AppServicesScope;
            var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>>();
            var intrinsicTask = await factory.TryCreateIntrinsicAsync(Current.IllustrationViewModel, Current.OriginalImageStream!, path);
            App.AppViewModel.DownloadManager.QueueTask(intrinsicTask);
            await intrinsicTask.Completion.Task;
        }

        await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(await AppKnownFolders.SavedWallPaper.GetFileAsync(guid));
        ToastNotificationHelper.ShowTextToastNotification(
            IllustrationViewerPageResources.SetAsSucceededTitle,
            IllustrationViewerPageResources.SetAsBackgroundSucceededTitle,
            AppContext.AppLogoNoCaptionUri);
    }

    private void SetAsBackgroundCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = !IsUgoira && Current.LoadingCompletedSuccessfully;
    }

    private void SetAsLockScreenCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = !IsUgoira && Current.LoadingCompletedSuccessfully;
    }

    private void ShareCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (Current.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
        {
            ShowAndHide(IllustrationViewerPageResources.CannotShareImageForNowTitle, Severity.Warning,
                IllustrationViewerPageResources.CannotShareImageForNowContent);
            return;
        }

        UIHelper.ShowShareUI();
    }

    private async void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(Current.IllustrationViewModel.Id));
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var link = MakoHelper.GenerateIllustrationWebUri(Current.IllustrationViewModel.Id).ToString();
        UIHelper.SetClipboardContent(package => package.SetText(link));
        ShowAndHide(IllustrationViewerPageResources.WebLinkCopiedToClipboardToastTitle);
    }

    private async void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        await Current.IllustrationViewModel.SaveAsAsync();
    }

    private async void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        await Current.IllustrationViewModel.SaveAsync();
    }

    private void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        FirstImageViewerPageViewModel!.SwitchBookmarkState();
        // update manually
        BookmarkCommand.Label = FirstIllustrationViewModel!.IsBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
        BookmarkCommand.IconSource = MakoHelper.GetBookmarkButtonIconSource(FirstIllustrationViewModel.IsBookmarked);
    }

    private void PlayGifCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = IsUgoira && (Current.LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false);
    }

    private void PlayGifCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var bitmap = Current.OriginalImageSource.To<BitmapImage>();
        if (bitmap.IsPlaying)
        {
            bitmap.Stop();
            PlayGifCommand.Description = PlayGifCommand.Label = IllustrationViewerPageResources.PlayGif;
            PlayGifCommand.IconSource = new SymbolIconSource { Symbol = Symbol.Play };
        }
        else
        {
            bitmap.Play();
            PlayGifCommand.Description = PlayGifCommand.Label = IllustrationViewerPageResources.PauseGif;
            PlayGifCommand.IconSource = new SymbolIconSource { Symbol = Symbol.Stop };
        }
    }

    private async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs e)
    {
        //UIHelper.SetClipboardContent(package =>
        //{
        //    //todo package.RequestedOperation = DataPackageOperation.Copy;
        //    Current.OriginalImageStream!.Seek(0);
        //    var stream = Current.OriginalImageStream.EncodeBitmapStreamAsync(false).GetAwaiter().GetResult(); 
        //    var streamRef = RandomAccessStreamReference.CreateFromStream(stream);
        //    package.SetBitmap(streamRef);
        //});
        await UIHelper.SetClipboardContentAsync(async package =>
        {
            package.RequestedOperation = DataPackageOperation.Copy;
            var file = await AppKnownFolders.CreateTemporaryFileWithNameAsync(GetCopyContentFileName(), IsUgoira ? "gif" : "png");
            await Current.OriginalImageStream!.SaveToFileAsync(file);
            var streamRef = RandomAccessStreamReference.CreateFromFile(file);
            package.SetBitmap(streamRef);
        });

        string GetCopyContentFileName() => $"{IllustrationId}{(IsUgoira ? "" : IsManga ? $"_p{CurrentIndex}" : "")}";
    }

    public ImageViewerPageViewModel Next()
    {
        ReassignAndResubscribeZoomingEvent(ImageViewerPageViewModels![CurrentIndex++]);
        return Current;
    }

    public ImageViewerPageViewModel Prev()
    {
        ReassignAndResubscribeZoomingEvent(ImageViewerPageViewModels![CurrentIndex--]);
        return Current;
    }

    private void ReassignAndResubscribeZoomingEvent(ImageViewerPageViewModel newViewModel)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Current is not null)
        {
            Current.ZoomChanged -= CurrentOnZoomChanged;
        }
        Current = newViewModel;
        Current.ZoomChanged += CurrentOnZoomChanged;
        CurrentOnZoomChanged(null, Current.Scale); // trigger as new viewmodel attached
    }

    // what a trash code...
    private void CurrentOnZoomChanged(object? sender, double e)
    {
        ZoomChanged?.Invoke(this, e);
    }


    public Task PostPublicBookmarkAsync()
    {
        // changes the IsBookmarked property of the item that of in the thumbnail list
        // so the thumbnail item will also receive state update 
        if (IllustrationViewModelInTheGridView is not null)
        {
            IllustrationViewModelInTheGridView.IsBookmarked = true;
        }

        return FirstIllustrationViewModel!.PostPublicBookmarkAsync();
    }

    public Task RemoveBookmarkAsync()
    {
        if (IllustrationViewModelInTheGridView is not null)
        {
            IllustrationViewModelInTheGridView.IsBookmarked = false;
        }

        return FirstIllustrationViewModel!.RemoveBookmarkAsync();
    }

    public async Task LoadUserProfile()
    {
        if (FirstIllustrationViewModel!.Illustration.User is { } userInfo && UserProfileImageSource is null)
        {
            UserInfo = userInfo;
            if (userInfo.ProfileImageUrls?.Medium is { } profileImage)
            {
                UserProfileImageSource = await App.AppViewModel.MakoClient.DownloadSoftwareBitmapSourceResultAsync(profileImage)
                    .GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync());
            }
        }
    }

    #region Commands

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

    public XamlUICommand BookmarkCommand { get; private set; } = null!; // the null-state is transient

    public XamlUICommand SaveCommand { get; } = IllustrationViewerPageResources.Save.GetCommand(
        FontIconSymbols.SaveE74E, VirtualKeyModifiers.Control, VirtualKey.S);

    public XamlUICommand SaveAsCommand { get; } = IllustrationViewerPageResources.SaveAs.GetCommand(
        FontIconSymbols.SaveAsE792, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.S);

    public XamlUICommand SetAsCommand { get; } = IllustrationViewerPageResources.SetAs.GetCommand(FontIconSymbols.PersonalizeE771);

    public XamlUICommand AddToBookmarkCommand { get; } = IllustrationViewerPageResources.AddToBookmark.GetCommand(FontIconSymbols.BookmarksE8A4);

    public XamlUICommand GenerateLinkCommand { get; } = IllustrationViewerPageResources.GenerateLink.GetCommand(FontIconSymbols.LinkE71B);

    public XamlUICommand GenerateWebLinkCommand { get; } = IllustrationViewerPageResources.GenerateWebLink.GetCommand(FontIconSymbols.PreviewLinkE8A1);

    public XamlUICommand OpenInWebBrowserCommand { get; } = IllustrationViewerPageResources.OpenInWebBrowser.GetCommand(FontIconSymbols.WebSearchF6FA);

    public StandardUICommand ShareCommand { get; } = new(StandardUICommandKind.Share);

    public XamlUICommand ShowQrCodeCommand { get; } = IllustrationViewerPageResources.ShowQRCode.GetCommand(FontIconSymbols.QRCodeED14);

    public XamlUICommand SetAsLockScreenCommand { get; } = new() { Label = IllustrationViewerPageResources.LockScreen };

    public XamlUICommand SetAsBackgroundCommand { get; } = new() { Label = IllustrationViewerPageResources.Background };

    public XamlUICommand FullScreenCommand { get; } = IllustrationViewerPageResources.FullScreen.GetCommand(FontIconSymbols.FullScreenE740);

    public XamlUICommand RestoreResolutionCommand { get; } = IllustrationViewerPageResources.RestoreOriginalResolution.GetCommand(FontIconSymbols.WebcamE8B8);

    #endregion

    #region Helper Functions

    public Visibility CalculateNextImageButtonVisibility(int index)
    {
        if (IllustrationView is null)
        {
            return Visibility.Collapsed;
        }

        return index < ImageViewerPageViewModels!.Length - 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    public Visibility CalculatePrevImageButtonVisibility(int index)
    {
        if (IllustrationView is null)
        {
            return Visibility.Collapsed;
        }

        return index > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public Visibility CalculateNextIllustrationButtonVisibility(int index)
    {
        if (ContainerGridViewModel is null)
        {
            return Visibility.Collapsed;
        }

        return ContainerGridViewModel.DataProvider.IllustrationsView.Count > IllustrationIndex + 1
            ? CalculateNextImageButtonVisibility(index).Inverse()
            : Visibility.Collapsed;
    }

    public Visibility CalculatePrevIllustrationButtonVisibility(int index)
    {
        if (ContainerGridViewModel is null)
        {
            return Visibility.Collapsed;
        }

        return IllustrationIndex > 0
            ? CalculatePrevImageButtonVisibility(index).Inverse()
            : Visibility.Collapsed;
    }

    #endregion

    #region SnackBar

    [ObservableProperty] private string _snackBarTitle = "";

    [ObservableProperty] private string _snackBarSubtitle = "";

    [ObservableProperty] private FontIconSource _snackBarIconSource = null!;

    [ObservableProperty] private bool _isSnackBarOpen;

    /// <remarks>
    /// Value type members require property to enable thread sharing
    /// </remarks>
    private static DateTime HideSnakeBarTime { get; set; }

    /// <summary>
    /// Show SnackBar
    /// </summary>
    /// <param name="message"><see cref="TeachingTip.Title"/></param>
    /// <param name="severity"><see cref="TeachingTip.IconSource"/></param>
    /// <param name="hint"><see cref="TeachingTip.Subtitle"/></param>
    public void Show(string message, Severity severity = Severity.Ok, string hint = "")
    {
        SnackBarTitle = message;
        SnackBarSubtitle = hint;
        SnackBarIconSource = new()
        {
            Glyph = severity switch
            {
                Severity.Ok => "\xE10B", // Accept
                Severity.Information => "\xE946", // Info
                Severity.Important => "\xE171", // Important
                Severity.Warning => "\xE7BA", // Warning
                Severity.Error => "\xEA39", // ErrorBadge
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<Severity, string>(severity)
            }
        };

        IsSnackBarOpen = true;
    }

    /// <summary>
    /// Show SnackBar and hide after <paramref name="mSec"/> microseconds
    /// </summary>
    /// <param name="message"><see cref="TeachingTip.Title"/></param>
    /// <param name="severity"><see cref="TeachingTip.IconSource"/></param>
    /// <param name="hint"><see cref="TeachingTip.Subtitle"/></param>
    /// <param name="mSec">Automatically hide after <paramref name="mSec"/> milliseconds</param>
    public async void ShowAndHide(string message, Severity severity = Severity.Ok, string hint = "", int mSec = 3000)
    {
        HideSnakeBarTime = DateTime.Now + TimeSpan.FromMilliseconds(mSec - 100);

        Show(message, severity, hint);

        await Task.Delay(mSec);

        if (DateTime.Now > HideSnakeBarTime)
            IsSnackBarOpen = false;
    }

    /// <summary>
    /// Snack bar severity on <see cref="TeachingTip.IconSource"/> (Segoe Fluent Icons font)
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Accept (E10B)
        /// </summary>
        Ok,
        /// <summary>
        /// Info (E946)
        /// </summary>
        Information,
        /// <summary>
        /// Important (E171)
        /// </summary>
        Important,
        /// <summary>
        /// Warning (E7BA)
        /// </summary>
        Warning,
        /// <summary>
        /// ErrorBadge (EA39)
        /// </summary>
        Error
    }

    #endregion
}
