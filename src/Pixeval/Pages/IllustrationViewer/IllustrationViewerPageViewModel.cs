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
using Windows.System;
using Windows.System.UserProfile;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Popups;
using Pixeval.UserControls;
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
    private bool _isGenerateLinkTeachingTipOpen;

    [ObservableProperty]
    private bool _isInfoPaneOpen;

    private ImageSource? _qrCodeSource;

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

    private readonly IllustrationViewModel[] _illustrations;

    public bool IsDisposed;

    // illustrations should contains only one item if the illustration is a single
    // otherwise it contains the entire manga data
    public IllustrationViewerPageViewModel(IllustrationGrid gridView, params IllustrationViewModel[] illustrations) : this(illustrations)
    {
        IllustrationGrid = gridView;
        ContainerGridViewModel = gridView.ViewModel;
        IllustrationViewModelInTheGridView = ContainerGridViewModel.IllustrationsView.Cast<IllustrationViewModel>().First(model => model.Id == Current.IllustrationViewModel.Id);
    }

    public IllustrationViewerPageViewModel(params IllustrationViewModel[] illustrations)
    {
        _illustrations = illustrations;
        ImageViewerPageViewModels = illustrations.Select(i => new ImageViewerPageViewModel(this, i)).ToArray();
        Current = ImageViewerPageViewModels[CurrentIndex];
        InitializeCommands();
        _ = LoadUserProfile();
    }

    /// <summary>
    /// Copy a new view model
    /// </summary>
    /// <returns></returns>
    public IllustrationViewerPageViewModel CreateNew()
    {
        return IllustrationGrid is not null ? new IllustrationViewerPageViewModel(IllustrationGrid, _illustrations) : new IllustrationViewerPageViewModel(_illustrations);
    }

    /// <summary>
    ///     The view model of the GridView that the <see cref="ImageViewerPageViewModels" /> comes from
    /// </summary>
    public IllustrationGridViewModel? ContainerGridViewModel { get; }

    /// <summary>
    ///     The <see cref="IllustrationGrid" /> that owns <see cref="ContainerGridViewModel" />
    /// </summary>
    public IllustrationGrid? IllustrationGrid { get; }

    /// <summary>
    ///     The <see cref="IllustrationViewModelInTheGridView" /> in <see cref="IllustrationGrid" /> that corresponds to
    ///     current
    ///     <see cref="IllustrationViewerPageViewModel" />
    /// </summary>
    public IllustrationViewModel? IllustrationViewModelInTheGridView { get; }

    /// <summary>
    ///     The index of current illustration in <see cref="IllustrationGrid" />
    /// </summary>
    public int? IllustrationIndex => ContainerGridViewModel?.IllustrationsView.IndexOf(IllustrationViewModelInTheGridView);

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

        (_userProfileImageSource as SoftwareBitmapSource)?.Dispose();
        _userProfileImageSource = null;
        IsDisposed = true;
    }

    public void UpdateCommandCanExecute()
    {
        PlayGifCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
    }

    private void InitializeCommands()
    {
        BookmarkCommand = new XamlUICommand
        {
            KeyboardAccelerators =
            {
                new KeyboardAccelerator
                {
                    Modifiers = VirtualKeyModifiers.Control,
                    Key = VirtualKey.D
                }
            },
            Label = FirstIllustrationViewModel!.IsBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark,
            IconSource = MakoHelper.GetBookmarkButtonIconSource(FirstIllustrationViewModel.IsBookmarked)
        };

        BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

        CopyCommand.CanExecuteRequested += CopyCommandOnCanExecuteRequested;
        CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;

        PlayGifCommand.CanExecuteRequested += PlayGifCommandOnCanExecuteRequested;
        PlayGifCommand.ExecuteRequested += PlayGifCommandOnExecuteRequested;

        ZoomOutCommand.ExecuteRequested += (_, _) => Current.Zoom(-0.5f);
        ZoomInCommand.ExecuteRequested += (_, _) => Current.Zoom(0.5f);

        SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;
        SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;

        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;
        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;
        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;
        ShareCommand.ExecuteRequested += ShareCommandOnExecuteRequested;
        ShowQrCodeCommand.ExecuteRequested += ShowQrCodeCommandOnExecuteRequested;

        SetAsLockScreenCommand.CanExecuteRequested += SetAsLockScreenCommandOnCanExecuteRequested;
        SetAsLockScreenCommand.ExecuteRequested += SetAsLockScreenCommandOnExecuteRequested;

        SetAsBackgroundCommand.CanExecuteRequested += SetAsBackgroundCommandOnCanExecuteRequested;
        SetAsBackgroundCommand.ExecuteRequested += SetAsBackgroundCommandOnExecuteRequested;
    }

    private async void SetAsBackgroundCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (Current.OriginalImageStream is null)
        {
            App.AppViewModel.ShowSnack(IllustrationViewerPageResources.OriginalmageStreamIsEmptyContent, 5000);
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
            App.AppViewModel.ShowSnack(IllustrationViewerPageResources.OriginalmageStreamIsEmptyContent, 5000);
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

    private async void ShowQrCodeCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        _qrCodeSource ??= await UIHelper.GenerateQrCodeForUrlAsync(MakoHelper.GenerateIllustrationWebUri(Current.IllustrationViewModel.Id).ToString());

        PopupManager.ShowPopup(PopupManager.CreatePopup(new QrCodePresenter(_qrCodeSource), lightDismiss: true));
    }

    private async void ShareCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (Current.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
        {
            await MessageDialogBuilder.CreateAcknowledgement(CurrentContext.Window, IllustrationViewerPageResources.CannotShareImageForNowTitle, IllustrationViewerPageResources.CannotShareImageForNowContent)
                .ShowAsync();
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
        App.AppViewModel.ShowSnack(IllustrationViewerPageResources.WebLinkCopiedToClipboardToastTitle, 5000);
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink)
        {
            IsGenerateLinkTeachingTipOpen = true;
        }

        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustrationAppUri(Current.IllustrationViewModel.Id).ToString()));
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
        var bitmap = (BitmapImage)Current.OriginalImageSource!;
        if (bitmap.IsPlaying)
        {
            bitmap.Stop();
            PlayGifCommand.Label = IllustrationViewerPageResources.PlayGif;
            PlayGifCommand.IconSource = new SymbolIconSource
            {
                Symbol = Symbol.Play
            };
        }
        else
        {
            bitmap.Play();
            PlayGifCommand.Label = IllustrationViewerPageResources.PauseGif;
            PlayGifCommand.IconSource = new SymbolIconSource
            {
                Symbol = Symbol.Stop
            };
        }
    }

    private void CopyCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = Current.LoadingCompletedSuccessfully;
    }

    private async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        await UIHelper.SetClipboardContentAsync(async package =>
        {
            package.RequestedOperation = DataPackageOperation.Copy;
            var file = await AppKnownFolders.CreateTemporaryFileWithNameAsync(GetCopyContentFileName(), IsUgoira ? "gif" : "png");
            await Current.OriginalImageStream!.SaveToFileAsync(file);
            package.SetStorageItems(Enumerates.ArrayOf(file), true);
        });

        string GetCopyContentFileName()
        {
            return $"{IllustrationId}{(IsUgoira ? string.Empty : IsManga ? $"_p{CurrentIndex}" : string.Empty)}";
        }
    }

    public ImageViewerPageViewModel Next()
    {
        Current = ImageViewerPageViewModels![++CurrentIndex];
        return Current;
    }

    public ImageViewerPageViewModel Prev()
    {
        Current = ImageViewerPageViewModels![--CurrentIndex];
        return Current;
    }

    public Task PostPublicBookmarkAsync()
    {
        // changes the IsBookmarked property of the item that of in the thumbnail list
        // so the thumbnail item will also receives state update 
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

    public StandardUICommand CopyCommand { get; } = new(StandardUICommandKind.Copy);

    public StandardUICommand PlayGifCommand { get; } = new(StandardUICommandKind.Play)
    {
        // The gif will be played as soon as its loaded, so the default state is playing and thus we need the button to be pause
        Label = IllustrationViewerPageResources.PauseGif,
        IconSource = new SymbolIconSource
        {
            Symbol = Symbol.Stop
        }
    };

    public XamlUICommand ZoomOutCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.ZoomOut,
        IconSource = new SymbolIconSource
        {
            Symbol = Symbol.ZoomOut
        },
        KeyboardAccelerators =
        {
            new KeyboardAccelerator
            {
                Key = VirtualKey.Subtract
            }
        }
    };

    public XamlUICommand ZoomInCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.ZoomIn,
        IconSource = new SymbolIconSource
        {
            Symbol = Symbol.ZoomIn
        },
        KeyboardAccelerators =
        {
            new KeyboardAccelerator
            {
                Key = VirtualKey.Add
            }
        }
    };

    public XamlUICommand BookmarkCommand { get; private set; } = null!; // the null-state is transient

    public StandardUICommand SaveCommand { get; } = new(StandardUICommandKind.Save) { Description = MiscResources.Save };

    public XamlUICommand SaveAsCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.SaveAs,
        KeyboardAccelerators =
        {
            new KeyboardAccelerator
            {
                Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift,
                Key = VirtualKey.S
            }
        },
        IconSource = FontIconSymbols.SaveAsE792.GetFontIconSource()
    };

    public XamlUICommand AddToBookmarkCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.AddToBookmark,
        IconSource = FontIconSymbols.BookmarksE8A4.GetFontIconSource()
    };

    public XamlUICommand GenerateLinkCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.GenerateLink,
        IconSource = FontIconSymbols.LinkE71B.GetFontIconSource()
    };

    public XamlUICommand GenerateWebLinkCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.GenerateWebLink,
        IconSource = FontIconSymbols.PreviewLinkE8A1.GetFontIconSource()
    };

    public XamlUICommand OpenInWebBrowserCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.OpenInWebBrowser,
        IconSource = FontIconSymbols.WebSearchF6FA.GetFontIconSource()
    };

    public StandardUICommand ShareCommand { get; } = new(StandardUICommandKind.Share);

    public XamlUICommand ShowQrCodeCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.ShowQRCode,
        IconSource = FontIconSymbols.QRCodeED14.GetFontIconSource()
    };

    public XamlUICommand SetAsLockScreenCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.LockScreen
    };

    public XamlUICommand SetAsBackgroundCommand { get; } = new()
    {
        Label = IllustrationViewerPageResources.Background
    };

    #endregion

    #region Helper Functions

    public Visibility CalculateNextImageButtonVisibility(int index)
    {
        if (IllustrationGrid is null)
        {
            return Visibility.Collapsed;
        }

        return index < ImageViewerPageViewModels!.Length - 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    public Visibility CalculatePrevImageButtonVisibility(int index)
    {
        if (IllustrationGrid is null)
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

        return ContainerGridViewModel.IllustrationsView.Count > IllustrationIndex + 1
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
}
