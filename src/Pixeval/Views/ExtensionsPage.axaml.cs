// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter =
            [
                new("DLL") { Patterns = ["*.dll"] },
                new("ZIP") { Patterns = ["*.zip"] }
            ],
            AllowMultiple = true
        });

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file.Path.LocalPath);
            var fileName = fileInfo.Name;
            switch (fileInfo.Extension.ToLowerInvariant())
            {
                case ".dll":
                {
                    var newDllPath = Path.Combine(AppInfo.ExtensionsFolder, fileName);
                    if (File.Exists(newDllPath))
                    {
                        viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.DllFileExistedError), fileName);
                        continue;
                    }

                    try
                    {
                        var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();

                        _ = fileInfo.CopyTo(newDllPath);
                        if (ExtensionService.TryLoadHost(newDllPath, logger, out var isOutdated))
                        {
                            viewContainer.ShowSuccess(I18NManager.GetResource(ExtensionsPageResources.DllLoadedSuccessfully), fileName);
                            continue;
                        }

                        viewContainer.ShowError(
                            I18NManager.GetResource(isOutdated
                                ? ExtensionsPageResources.ExtensionOutdated
                                : ExtensionsPageResources.DllLoadFailed), fileName);
                    }
                    catch (Exception ex)
                    {
                        viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.DllLoadFailed), $"{fileName}: {ex.Message}");
                    }

                    break;
                }
                case ".zip":
                    try
                    {
                        var dllNames = await Task.Run(() =>
                        {
                            using var zipArchive = ZipFile.OpenRead(fileInfo.FullName);
                            return zipArchive.Entries.Select(t => t.FullName)
                                .Where(t => !t.Contains('\\') && !t.Contains('/') && t.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                                .ToArray();
                        });

                        if (dllNames.Length is not 0)
                        {
                            await Task.Run(() =>
                                ZipFile.ExtractToDirectory(fileInfo.FullName, AppInfo.ExtensionsFolder));

                            foreach (var dllName in dllNames)
                                try
                                {
                                    var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();

                                    var newDllPath = Path.Combine(AppInfo.ExtensionsFolder, dllName);
                                    if (ExtensionService.TryLoadHost(newDllPath, logger, out var isOutdated))
                                    {
                                        viewContainer.ShowSuccess(I18NManager.GetResource(ExtensionsPageResources.DllLoadedSuccessfully), fileName);
                                        continue;
                                    }

                                    viewContainer.ShowError(
                                        I18NManager.GetResource(isOutdated
                                            ? ExtensionsPageResources.ExtensionOutdated
                                            : ExtensionsPageResources.DllLoadFailed), fileName);
                                }
                                catch (Exception ex)
                                {
                                    viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.DllLoadFailed), $"{fileName}: {ex.Message}");
                                }
                        }
                        else
                            viewContainer.ShowWarning(I18NManager.GetResource(ExtensionsPageResources.ZipContainsNoDll), fileName);
                    }
                    catch (Exception ex)
                    {
                        viewContainer.ShowError(I18NManager.GetResource(ExtensionsPageResources.UnzipFailed), $"{fileName}: {ex.Message}");
                    }

                    break;
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
