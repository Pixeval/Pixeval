#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationViewerPageViewModel.cs
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
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Download;
using Pixeval.Popups;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using QRCoder;

namespace Pixeval.Pages.IllustrationViewer
{
    public class IllustrationViewerPageViewModel : ObservableObject, IDisposable
    {
        private ImageViewerPageViewModel _current = null!;

        private int _currentIndex;

        private bool _isGenerateLinkTeachingTipOpen;

        private bool _isInfoPaneOpen;

        private ImageSource? _qrCodeSource;

        private ImageSource? _userProfileImageSource;

        // Remarks:
        // illustrations should contains only one item if the illustration is a single
        // otherwise it contains the entire manga data
        public IllustrationViewerPageViewModel(IllustrationGrid gridView, params IllustrationViewModel[] illustrations)
        {
            ImageViewerPageViewModels = illustrations.Select(i => new ImageViewerPageViewModel(this, i)).ToArray();
            Current = ImageViewerPageViewModels[CurrentIndex];
            IllustrationGrid = gridView;
            ContainerGridViewModel = gridView.ViewModel;
            IllustrationViewModelInTheGridView = ContainerGridViewModel.IllustrationsView.Cast<IllustrationViewModel>().First(model => model.Id == Current.IllustrationViewModel.Id);
            InitializeCommands();
            _ = LoadUserProfileImage();
        }

        public IllustrationViewerPageViewModel(params IllustrationViewModel[] illustrations)
        {
            ImageViewerPageViewModels = illustrations.Select(i => new ImageViewerPageViewModel(this, i)).ToArray();
            Current = ImageViewerPageViewModels[CurrentIndex];
            InitializeCommands();
            _ = LoadUserProfileImage();
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

        public ImageViewerPageViewModel[] ImageViewerPageViewModels { get; }

        public int CurrentIndex
        {
            get => _currentIndex;
            private set => SetProperty(ref _currentIndex, value);
        }

        public ImageViewerPageViewModel Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        // Remarks:
        // The reason why we don't put UserProfileImageSource into IllustrationViewModel
        // is because the whole array of Illustrations is just representing the same 
        // illustration's different manga pages, so all of them have the same illustrator
        // If the UserProfileImageSource is in IllustrationViewModel and the illustration
        // itself is a manga then all of the IllustrationViewModel in Illustrations will
        // request the same user profile image which is pointless and will (inevitably) causing
        // the waste of system resource
        public ImageSource? UserProfileImageSource
        {
            get => _userProfileImageSource;
            set => SetProperty(ref _userProfileImageSource, value);
        }

        public string IllustrationId => FirstIllustrationViewModel.Illustration.Id.ToString();

        public string? IllustratorName => FirstIllustrationViewModel.Illustration.User?.Name;

        public string? IllustratorUid => FirstIllustrationViewModel.Illustration.User?.Id.ToString();

        public bool IsManga => ImageViewerPageViewModels.Length > 1;

        public bool IsUgoira => Current.IllustrationViewModel.Illustration.IsUgoira();

        public IllustrationViewModel FirstIllustrationViewModel => FirstImageViewerPageViewModel.IllustrationViewModel;

        public ImageViewerPageViewModel FirstImageViewerPageViewModel => ImageViewerPageViewModels[0];

        public bool IsInfoPaneOpen
        {
            get => _isInfoPaneOpen;
            set => SetProperty(ref _isInfoPaneOpen, value);
        }

        public bool IsGenerateLinkTeachingTipOpen
        {
            get => _isGenerateLinkTeachingTipOpen;
            set => SetProperty(ref _isGenerateLinkTeachingTipOpen, value);
        }

        public void Dispose()
        {
            foreach (var imageViewerPageViewModel in ImageViewerPageViewModels)
            {
                imageViewerPageViewModel.Dispose();
            }

            (_userProfileImageSource as SoftwareBitmapSource)?.Dispose();
        }

        public void UpdateCommandCanExecute()
        {
            PlayGifCommand.NotifyCanExecuteChanged();
            CopyCommand.NotifyCanExecuteChanged();
            SaveMangaCommand.NotifyCanExecuteChanged();
            SaveMangaAsCommand.NotifyCanExecuteChanged();
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
                Label = FirstIllustrationViewModel.IsBookmarked ? IllustrationViewerPageResources.RemoveBookmark : IllustrationViewerPageResources.Bookmark,
                IconSource = GetBookmarkButtonIcon()
            };

            BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

            CopyCommand.CanExecuteRequested += CopyCommandOnCanExecuteRequested;
            CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;

            PlayGifCommand.CanExecuteRequested += PlayGifCommandOnCanExecuteRequested;
            PlayGifCommand.ExecuteRequested += PlayGifCommandOnExecuteRequested;

            ZoomOutCommand.ExecuteRequested += (_, _) => Current.Zoom(-0.5);
            ZoomInCommand.ExecuteRequested += (_, _) => Current.Zoom(0.5);

            SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;
            SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;

            SaveMangaCommand.CanExecuteRequested += SaveMangaCommandOnCanExecuteRequested;
            SaveMangaCommand.ExecuteRequested += SaveMangaCommandOnExecuteRequested;

            SaveMangaAsCommand.CanExecuteRequested += SaveMangaAsCommandOnCanExecuteRequested;
            SaveMangaAsCommand.ExecuteRequested += SaveMangaAsCommandOnExecuteRequested;

            GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;
            GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;
            OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;
            ShareCommand.ExecuteRequested += ShareCommandOnExecuteRequested;
            ShowQrCodeCommand.ExecuteRequested += ShowQRCodeCommandOnExecuteRequested;
        }

        private async void ShowQRCodeCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (_qrCodeSource is null)
            {
                var qrCodeGen = new QRCodeGenerator();
                var urlPayload = new PayloadGenerator.Url(MakoHelper.GetIllustrationWebUri(Current.IllustrationViewModel.Id).ToString());
                var qrCodeData = qrCodeGen.CreateQrCode(urlPayload, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new BitmapByteQRCode(qrCodeData);
                var bytes = qrCode.GetGraphic(20);
                _qrCodeSource = await (await IOHelper.GetRandomAccessStreamFromByteArrayAsync(bytes)).GetBitmapImageAsync(true);
            }

            PopupManager.ShowPopup(PopupManager.CreatePopup(new QrCodePresenter(_qrCodeSource), lightDismiss: true));
        }

        private async void ShareCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (Current.LoadingOriginalSourceTask is not { IsCompletedSuccessfully: true })
            {
                await MessageDialogBuilder.CreateAcknowledgement(App.AppViewModel.Window, IllustrationViewerPageResources.CannotShareImageForNowTitle, IllustrationViewerPageResources.CannotShareImageForNowContent)
                    .ShowAsync();
                return;
            }

            UIHelper.ShowShareUI();
        }

        private async void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            await Launcher.LaunchUriAsync(MakoHelper.GetIllustrationWebUri(Current.IllustrationViewModel.Id));
        }

        private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            var link = MakoHelper.GetIllustrationWebUri(Current.IllustrationViewModel.Id).ToString();
            UIHelper.SetClipboardContent(package => package.SetText(link));
            UIHelper.ShowTextToastNotification(IllustrationViewerPageResources.WebLinkCopiedToClipboardToastTitle, link);
        }

        private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink)
            {
                IsGenerateLinkTeachingTipOpen = true;
            }

            UIHelper.SetClipboardContent(package => package.SetText(AppContext.GenerateAppLinkToIllustration(Current.IllustrationViewModel.Id).ToString()));
        }

        private void SaveMangaAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void SaveMangaAsCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = IsManga;
        }

        private void SaveMangaCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void SaveMangaCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = IsManga;
        }

        private void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private async void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            // TODO 
            var downloadTask = await DownloadFactories.Illustration.CreateAsync(Current.IllustrationViewModel, App.AppViewModel.AppSetting.DefaultDownloadPathMacro);
            App.AppViewModel.IllustrationDownloadManager.QueueTask(downloadTask);
        }

        private void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            FirstImageViewerPageViewModel.SwitchBookmarkState();
            // update manually
            BookmarkCommand.Label = FirstIllustrationViewModel.IsBookmarked ? IllustrationViewerPageResources.RemoveBookmark : IllustrationViewerPageResources.Bookmark;
            BookmarkCommand.IconSource = GetBookmarkButtonIcon();
        }

        private IconSource GetBookmarkButtonIcon()
        {
            var systemThemeFontFamily = new FontFamily("Segoe MDL2 Assets");
            return FirstIllustrationViewModel.IsBookmarked
                ? new FontIconSource
                {
                    Glyph = "\xEB52", // HeartFill
                    Foreground = new SolidColorBrush(Colors.Crimson),
                    FontFamily = systemThemeFontFamily
                }
                : new FontIconSource
                {
                    Glyph = "\xEB51", // Heart
                    FontFamily = systemThemeFontFamily
                };
        }

        private void PlayGifCommandOnCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = IsUgoira && (Current.LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false);
        }

        private void PlayGifCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            var bitmap = (BitmapImage) Current.OriginalImageSource!;
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
            args.CanExecute = Current.LoadingOriginalSourceTask?.IsCompletedSuccessfully ?? false;
        }

        private async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            await UIHelper.SetClipboardContentAsync(async package =>
            {
                package.RequestedOperation = DataPackageOperation.Copy;
                var file = await AppContext.CreateTemporaryFileWithNameAsync(GetCopyContentFileName(), IsUgoira ? "gif" : "png");
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
            Current = ImageViewerPageViewModels[++CurrentIndex];
            return Current;
        }

        public ImageViewerPageViewModel Prev()
        {
            Current = ImageViewerPageViewModels[--CurrentIndex];
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

            return FirstIllustrationViewModel.PostPublicBookmarkAsync();
        }

        public Task RemoveBookmarkAsync()
        {
            if (IllustrationViewModelInTheGridView is not null)
            {
                IllustrationViewModelInTheGridView.IsBookmarked = false;
            }

            return FirstIllustrationViewModel.RemoveBookmarkAsync();
        }

        private async Task LoadUserProfileImage()
        {
            if (FirstIllustrationViewModel.Illustration.User?.ProfileImageUrls?.Medium is { } profileImage)
            {
                UserProfileImageSource = await App.AppViewModel.MakoClient.DownloadSoftwareBitmapSourceResultAsync(profileImage)
                    .GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync());
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

        public StandardUICommand SaveCommand { get; } = new(StandardUICommandKind.Save);

        public XamlUICommand SaveAsCommand { get; } = new()
        {
            Label = IllustrationViewerPageResources.SaveAs,
            KeyboardAccelerators =
            {
                new KeyboardAccelerator
                {
                    Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, // todo
                    Key = VirtualKey.S
                }
            },
            IconSource = FontIconSymbols.SaveAsE792.GetFontIconSource()
        };

        public XamlUICommand SaveMangaCommand { get; } = new()
        {
            Label = IllustrationViewerPageResources.SaveManga,
            IconSource = FontIconSymbols.SaveCopyEA35.GetFontIconSource()
        };

        public XamlUICommand SaveMangaAsCommand { get; } = new()
        {
            Label = IllustrationViewerPageResources.SaveMangaAs,
            IconSource = FontIconSymbols.LinkE71B.GetFontIconSource()
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

        #endregion

        #region Helper Functions

        public Visibility CalculateNextImageButtonVisibility(int index)
        {
            if (IllustrationGrid is null)
            {
                return Visibility.Collapsed;
            }

            return index < ImageViewerPageViewModels.Length - 1 ? Visibility.Visible : Visibility.Collapsed;
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
}