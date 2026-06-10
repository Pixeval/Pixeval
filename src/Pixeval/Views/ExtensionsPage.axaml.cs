// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;

namespace Pixeval.Views;

public partial class ExtensionsPage : ContentPage
{
    private ExtensionService ExtensionService { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    private bool _firstTime = true;

    public ObservableCollection<ExtensionsHostModel> Models => ExtensionService.HostModels;

    public ExtensionsPage() => InitializeComponent();

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_firstTime && ExtensionService.OutDateExtensionHostsCount is var count and not 0)
        {
            _firstTime = false;
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
                I18NManager.GetResource(ExtensionsPageResources.SomeExtensionsOutdatedFormatted, count));
        }
    }

    private async void AddExtensionsOnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer, StorageProvider: { } provider })
            return;

        if (ExtensionService.NativeLibraryExtension is not { } extension)
        {
            viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.PlatformNotSupportExtensions));
            return;
        }

        var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();

        var files = await provider.OpenFilePickerAsync(new()
        {
            FileTypeFilter =
            [
                new("Zipped Extension") { Patterns = ["*.zip"] },
                new("Native Library Extension") { Patterns = ["*" + extension] }
            ],
            AllowMultiple = true
        });

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file.Path.LocalPath);
            var fileName = fileInfo.Name;
            switch (fileInfo.Extension.ToLowerInvariant())
            {
                case { } ext when ext == ExtensionService.NativeLibraryExtension:
                {
                    var newLibraryPath = Path.Combine(AppInfo.ExtensionsFolder, fileName);
                    if (File.Exists(newLibraryPath))
                    {
                        viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.ExtensionFileExistedError), fileName);
                        continue;
                    }

                    try
                    {
                        _ = fileInfo.CopyTo(newLibraryPath);
                    }
                    catch (Exception ex)
                    {
                        viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.ExtensionLoadFailed), $"{fileName}: {ex.Message}");
                    }

                    LoadExtension(newLibraryPath, fileName);

                    break;
                }
                case ".zip":
                    try
                    {
                        var plan = await Task.Run(() =>
                        {
                            using var zipArchive = ZipFile.OpenRead(fileInfo.FullName);
                            return ExtensionService.CreateExtensionZipExtractionPlan(
                                zipArchive,
                                fileInfo.FullName,
                                AppInfo.ExtensionsFolder);
                        });

                        if (plan.HostLibraryEntryNames.Count is not 0)
                        {
                            await Task.Run(() =>
                                ZipFile.ExtractToDirectory(fileInfo.FullName, plan.DestinationDirectory));

                            foreach (var libraryName in plan.HostLibraryEntryNames)
                            {
                                var newLibraryPath = Path.Combine(plan.DestinationDirectory, libraryName);
                                LoadExtension(newLibraryPath, fileName);
                            }
                        }
                        else
                            viewContainer.ShowWarning(I18NManager.GetResource(ExtensionsPageResources.ZipContainsNoExtension), fileName);
                    }
                    catch (Exception ex)
                    {
                        viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.ExtensionLoadFailed), $"{fileName}: {ex.Message}");
                    }

                    break;
            }
        }

        return;

        void LoadExtension(string libraryPath, string fileName)
        {
            try
            {
                if (ExtensionService.TryLoadHost(libraryPath, logger, out var outdatedVersion))
                {
                    viewContainer.ShowSuccess(I18NManager.GetResource(ExtensionsPageResources.ExtensionLoadedSuccessfully), fileName);
                    return;
                }

                viewContainer.ShowError(
                    outdatedVersion is null
                        ? I18NManager.GetResource(ExtensionsPageResources.ExtensionLoadFailed)
                        : I18NManager.GetResource(ExtensionsPageResources.ExtensionOutdatedFormatted, outdatedVersion),
                    fileName);
            }
            catch (Exception ex)
            {
                viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.ExtensionLoadFailed), $"{fileName}: {ex.Message}");
            }
        }
    }

    private void UnloadHostOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: ExtensionsHostModel model })
            ExtensionService.UnloadHost(model);
    }

    private void OpenExtensionsOnClick(object? sender, RoutedEventArgs e)
    {
        _ = TopLevel.GetTopLevel(this)?.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(AppInfo.ExtensionsFolder));
    }

    private void ExtensionsHelpOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: Uri uri })
            _ = TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(uri);
    }

    private void ExtensionCard_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(new Uri("https://github.com/Pixeval/Pixeval/releases"));
    }
}
