#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ImageViewerPageViewModel.cs
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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Input;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.IllustrationView;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Utilities.Threading;
using SixLabors.ImageSharp;

namespace Pixeval.Pages.IllustrationViewer;

public partial class ImageViewerPageViewModel : ObservableObject, IDisposable
{
    public enum LoadingPhase
    {
        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.CheckingCache))]
        CheckingCache,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.LoadingFromCache))]
        LoadingFromCache,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.DownloadingUgoiraZipFormatted), DownloadingUgoiraZip)]
        DownloadingUgoiraZip,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.MergingUgoiraFrames))]
        MergingUgoiraFrames,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.DownloadingImageFormatted), DownloadingImage)]
        DownloadingImage,

        [LocalizedResource(typeof(ImageViewerPageResources), nameof(ImageViewerPageResources.LoadingImage))]
        LoadingImage
    }

    private bool _disposed;

    [ObservableProperty]
    private bool _isMirrored;

    [ObservableProperty]
    private bool _isPlaying = true;

    [ObservableProperty]
    private double _loadingProgress;

    [ObservableProperty]
    private string? _loadingText;

    [ObservableProperty]
    private IReadOnlyList<int>? _msIntervals;

    [ObservableProperty]
    private IReadOnlyList<Stream>? _originalImageSources;

    [ObservableProperty]
    private int _rotationDegree;

    [ObservableProperty]
    private float _scale = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFit))]
    private ZoomableImageMode _showMode;

    private bool _loadSuccessfully;

    public bool LoadSuccessfully
    {
        get => _loadSuccessfully;
        private set
        {
            if (value == _loadSuccessfully)
                return;
            _loadSuccessfully = value;
            OnPropertyChanged();
        }
    }

    public ImageViewerPageViewModel(IllustrationViewerPageViewModel illustrationViewerPageViewModel, IllustrationItemViewModel illustrationViewModel)
    {
        IllustrationViewerPageViewModel = illustrationViewerPageViewModel;
        IllustrationViewModel = illustrationViewModel;
        _ = LoadImage();

        InitializeCommands();
    }

    /// <summary>
    /// <see cref="ShowMode"/> is <see cref="ZoomableImageMode.Fit"/> or not
    /// </summary>
    public bool IsFit => ShowMode is ZoomableImageMode.Fit;

    public async Task<Stream?> GetOriginalImageSourceAsync(bool useBmp, IProgress<int>? progress = null)
    {
        if (OriginalImageSources is null)
            return null;

        if (IllustrationViewModel.IsUgoira)
            return await OriginalImageSources.UgoiraSaveToStreamAsync(MsIntervals ?? [], progress);

        if (OriginalImageSources.FirstOrDefault() is { } stream)
        {
            stream.Position = 0;
            return await stream.IllustrationSaveToStreamAsync(useBmp ? IllustrationDownloadFormat.Bmp : null);
        }

        return null;
    }

    public CancellationHandle ImageLoadingCancellationHandle { get; } = new();

    /// <summary>
    ///     The view model of the <see cref="IllustrationViewerPage" /> that hosts the owner <see cref="ImageViewerPage" />
    ///     of this <see cref="ImageViewerPageViewModel" />
    /// </summary>
    public IllustrationViewerPageViewModel IllustrationViewerPageViewModel { get; }

    public IllustrationItemViewModel IllustrationViewModel { get; }

    public void Dispose()
    {
        _disposed = true;
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// see <see cref="ZoomableImage.Zoom"/>
    /// </summary>
    /// <param name="delta"></param>
    public void Zoom(float delta)
    {
        Scale = MathF.Exp(MathF.Log(Scale) + delta / 5000f);
    }

    private void AdvancePhase(LoadingPhase phase)
    {
        LoadingText = phase.GetLocalizedResource() switch
        {
            { FormatKey: LoadingPhase } attr => attr.GetLocalizedResourceContent()?.Format((int)LoadingProgress),
            var attr => attr?.GetLocalizedResourceContent()
        };
    }

    private void AddHistory()
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        _ = manager.Delete(x => x.Id == IllustrationViewModel.Id);
        manager.Insert(new BrowseHistoryEntry { Id = IllustrationViewModel.Id });
    }

    private async Task LoadImage()
    {
        if (!LoadSuccessfully || _disposed)
        {
            _disposed = false;
            _ = IllustrationViewModel.TryLoadThumbnail(this).ContinueWith(
                _ => OriginalImageSources ??= [IllustrationViewModel.ThumbnailStream!],
                TaskScheduler.FromCurrentSynchronizationContext());
            AddHistory();
            await LoadOriginalImage();
            IllustrationViewModel.UnloadThumbnail(this);
        }

        return;

        async Task LoadOriginalImage()
        {
            var cacheKey = await IllustrationViewModel.GetIllustrationOriginalImageCacheKeyAsync();
            AdvancePhase(LoadingPhase.CheckingCache);
            if (App.AppViewModel.AppSetting.UseFileCache && await App.AppViewModel.Cache.ExistsAsync(cacheKey))
            {
                AdvancePhase(LoadingPhase.LoadingFromCache);
                OriginalImageSources = await App.AppViewModel.Cache.GetAsync<IReadOnlyList<Stream>>(cacheKey);
                LoadSuccessfully = true;
            }
            else if (IllustrationViewModel.IsUgoira)
            {
                AdvancePhase(LoadingPhase.DownloadingUgoiraZip);
                var (metadata, url) = await IllustrationViewModel.GetUgoiraOriginalUrlAsync();
                var downloadRes = await App.AppViewModel.MakoClient.DownloadStreamAsync(url, new Progress<double>(d =>
                {
                    LoadingProgress = d;
                    AdvancePhase(LoadingPhase.DownloadingUgoiraZip);
                }), ImageLoadingCancellationHandle);
                switch (downloadRes)
                {
                    case Result<Stream>.Success(var zipStream):
                        AdvancePhase(LoadingPhase.MergingUgoiraFrames);
                        OriginalImageSources = [.. await IoHelper.ReadZipArchiveEntries(zipStream)];
                        MsIntervals = metadata.UgoiraMetadataInfo.Frames.Select(x => (int)x.Delay).ToArray();
                        break;
                    case Result<Stream>.Failure(OperationCanceledException):
                        return;
                }

                LoadSuccessfully = true;
            }
            else
            {
                var url = IllustrationViewModel.OriginalStaticUrl;
                AdvancePhase(LoadingPhase.DownloadingImage);
                var ras = await App.AppViewModel.MakoClient.DownloadStreamAsync(url, new Progress<double>(d =>
                {
                    LoadingProgress = d;
                    AdvancePhase(LoadingPhase.DownloadingImage);
                }), ImageLoadingCancellationHandle);
                switch (ras)
                {
                    case Result<Stream>.Success(var s):
                        OriginalImageSources = [s];
                        break;
                    default:
                        return;
                }

                LoadSuccessfully = true;
            }

            if (OriginalImageSources is not null && !_disposed)
            {
                UpdateCommandCanExecute();
                if (App.AppViewModel.AppSetting.UseFileCache)
                {
                    _ = await App.AppViewModel.Cache.TryAddAsync(cacheKey, OriginalImageSources, TimeSpan.FromDays(1));
                }

                return;
            }

            throw new IllustrationSourceNotFoundException(ImageViewerPageResources.CannotFindImageSourceContent);
        }
    }
    private void InitializeCommands()
    {
        SaveCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        SaveCommand.ExecuteRequested += (_, _) => IllustrationViewModel.SaveCommand.Execute((IllustrationViewerPageViewModel.WindowContent, (Func<IProgress<int>?, Task<Stream?>>)(p => GetOriginalImageSourceAsync(false, p))));

        SaveAsCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        SaveAsCommand.ExecuteRequested += (_, _) => IllustrationViewModel.SaveAsCommand.Execute((IllustrationViewerPageViewModel.Window, (Func<IProgress<int>?, Task<Stream?>>)(p => GetOriginalImageSourceAsync(false, p))));

        CopyCommand.CanExecuteRequested += LoadingCompletedCanExecuteRequested;
        CopyCommand.ExecuteRequested += (_, _) => IllustrationViewModel.CopyCommand.Execute((IllustrationViewerPageViewModel.WindowContent, (Func<IProgress<int>?, Task<Stream?>>)(p => GetOriginalImageSourceAsync(true, p))));
    }

    private void UpdateCommandCanExecute()
    {
        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        IllustrationViewerPageViewModel.UpdateCommandCanExecute();
    }

    private void LoadingCompletedCanExecuteRequested(XamlUICommand _, CanExecuteRequestedEventArgs args) => args.CanExecute = LoadSuccessfully;

    public XamlUICommand SaveCommand { get; } = IllustrationViewerPageResources.Save.GetCommand(
        FontIconSymbols.SaveE74E, VirtualKeyModifiers.Control, VirtualKey.S);

    public XamlUICommand SaveAsCommand { get; } = IllustrationViewerPageResources.SaveAs.GetCommand(
        FontIconSymbols.SaveAsE792, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.S);

    public XamlUICommand CopyCommand { get; } = IllustrationViewerPageResources.Copy.GetCommand(
        FontIconSymbols.CopyE8C8, VirtualKeyModifiers.Control, VirtualKey.C);

    private void DisposeInternal()
    {
        IllustrationViewModel.UnloadThumbnail(this);
        // if the loading task is null or hasn't been completed yet, the 
        // OriginalImageSources would be the thumbnail source, its disposal may 
        // cause the IllustrationGrid shows weird result such as an empty content
        if (LoadSuccessfully && OriginalImageSources is not null)
            foreach (var originalImageSource in OriginalImageSources)
            {
                originalImageSource?.Dispose();
            }

        OriginalImageSources = null;
        LoadSuccessfully = false;
    }
}
