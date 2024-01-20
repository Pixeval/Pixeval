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
using Windows.Foundation;
using Windows.Storage;
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
using Pixeval.Controls.IllustrationView;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Misc;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.IllustrationViewer;

public partial class IllustrationViewerPageViewModel : DetailedObservableObject, IDisposable
{
    public Window Window { get; set; } = null!;

    public FrameworkElement WindowContent => Window.Content.To<FrameworkElement>();

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
            illustrationViewModel.UnloadThumbnail(this);
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

    public NavigationViewTag<IllustrationInfoPage, IllustrationViewerPageViewModel> IllustrationInfoTag { get; } = new(null!);

    public NavigationViewTag<CommentsPage, (IAsyncEnumerable<Comment?>, long IllustrationId)> CommentsTag { get; } = new(default);

    public NavigationViewTag<RelatedWorksPage, long> RelatedWorksTag { get; } = new(default);

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

            _ = LoadUserProfile();

            CurrentPageIndex = 0;
            OnPropertyChanged(nameof(IsUgoira));
            OnDetailedPropertyChanged(oldValue, value, oldTag, CurrentPage.Id);
            OnPropertyChanged(nameof(CurrentIllustration));
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式", Justification = "<挂起>")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式", Justification = "<挂起>")]
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
        ShareCommand.NotifyCanExecuteChanged();
    }

    private void InitializeCommands()
    {
        ShareCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        ShareCommand.ExecuteRequested += ShareCommandExecuteRequested;

        SetAsLockScreenCommand.CanExecuteRequested += IsNotUgoiraAndLoadingCompletedCanExecuteRequested;
        SetAsLockScreenCommand.ExecuteRequested += SetAsLockScreenCommandOnExecuteRequested;

        SetAsBackgroundCommand.CanExecuteRequested += IsNotUgoiraAndLoadingCompletedCanExecuteRequested;
        SetAsBackgroundCommand.ExecuteRequested += SetAsBackgroundCommandOnExecuteRequested;

        FullScreenCommand.ExecuteRequested += (_, _) => IsFullScreen = !IsFullScreen;
    }

    private void LoadingCompletedCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = CurrentImage.LoadSuccessfully;
    }

    private void IsNotUgoiraAndLoadingCompletedCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
    {
        args.CanExecute = !IsUgoira && CurrentImage.LoadSuccessfully;
    }

    private void SetAsBackgroundCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        SetPersonalization(UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync);
    }

    private void SetAsLockScreenCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        SetPersonalization(UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync);
    }

    private async void SetPersonalization(Func<StorageFile, IAsyncOperation<bool>> operation)
    {
        if (CurrentImage.OriginalImageSources is not [{ } first, ..])
            return;

        var guid = await first.Sha1Async();
        if (await AppKnownFolders.SavedWallPaper.TryGetFileRelativeToSelfAsync(guid) is null)
        {
            var path = Path.Combine(AppKnownFolders.SavedWallPaper.Self.Path, guid);
            using var scope = App.AppViewModel.AppServicesScope;
            var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
            var intrinsicTask = await factory.TryCreateIntrinsicAsync(CurrentIllustration, first, path);
            App.AppViewModel.DownloadManager.QueueTask(intrinsicTask);
            await intrinsicTask.Completion.Task;
        }
        var file = await AppKnownFolders.SavedWallPaper.GetFileAsync(guid);
        _ = await operation(file);

        ToastNotificationHelper.ShowTextToastNotification(
            IllustrationViewerPageResources.SetAsSucceededTitle,
            IllustrationViewerPageResources.SetAsBackgroundSucceededTitle,
            AppContext.AppLogoNoCaptionUri);
    }

    private void ShareCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (CurrentImage.LoadSuccessfully)
            Window.ShowShareUi();
        else
            WindowContent.ShowTeachingTipAndHide(IllustrationViewerPageResources.CannotShareImageForNowTitle, TeachingTipSeverity.Warning, IllustrationViewerPageResources.CannotShareImageForNowContent);
    }

    public XamlUICommand IllustrationInfoAndCommentsCommand { get; } =
        IllustrationViewerPageResources.IllustrationInfoAndComments.GetCommand(FontIconSymbols.InfoE946, VirtualKey.F12);

    public XamlUICommand SetAsCommand { get; } = IllustrationViewerPageResources.SetAs.GetCommand(FontIconSymbols.PersonalizeE771);

    public StandardUICommand ShareCommand { get; } = new(StandardUICommandKind.Share);

    public XamlUICommand SetAsLockScreenCommand { get; } = new() { Label = IllustrationViewerPageResources.LockScreen };

    public XamlUICommand SetAsBackgroundCommand { get; } = new() { Label = IllustrationViewerPageResources.Background };

    public XamlUICommand FullScreenCommand { get; } = IllustrationViewerPageResources.FullScreen.GetCommand(FontIconSymbols.FullScreenE740);

    #endregion
}
