// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using Windows.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Extensions;
using Pixeval.Util.UI;
using WinUI3Utilities;
using System.IO.Compression;
using System.Linq;

namespace Pixeval.Pages.Misc;

public sealed partial class ExtensionsPage
{
    private ExtensionService ExtensionService { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    private IReadOnlyList<ExtensionsHostModel> Models => ExtensionService.HostModels;

    public ExtensionsPage() => InitializeComponent();

    private async void AddExtensionsOnClick(object sender, RoutedEventArgs e)
    {
        var files = await this.OpenMultipleDllsOpenPickerAsync();
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file.Path);
            var fileName = fileInfo.Name;
            switch (fileInfo.Extension)
            {
                case ".dll":
                {
                    var newDllPath = AppKnownFolders.Extensions.CombinePath(fileName);
                    if (File.Exists(newDllPath))
                    {
                        this.ErrorGrowl(ExtensionsPageResources.DllFileExistedError, fileName);
                        continue;
                    }

                    try
                    {
                        _ = fileInfo.CopyTo(newDllPath);
                        if (ExtensionService.TryLoadHost(newDllPath))
                        {
                            this.SuccessGrowl(ExtensionsPageResources.DllLoadedSuccessfully, fileName);
                            continue;
                        }

                        this.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, fileName);
                    }
                    catch (Exception ex)
                    {
                        this.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, $"{fileName}: {ex.Message}");
                    }

                    break;
                }
                case ".zip":
                    try
                    {
                        using var zipArchive = ZipFile.OpenRead(fileInfo.FullName);
                        var dllNames = zipArchive.Entries.Select(t => t.FullName)
                            .Where(t => !t.Contains('\\') && !t.Contains('/') && t.EndsWith(".dll")).ToArray();
                        if (dllNames.Length is not 0)
                        {
                            ZipFile.ExtractToDirectory(fileInfo.FullName, AppKnownFolders.Extensions.FullPath);
                            foreach (var dllName in dllNames)
                            {
                                try
                                {
                                    var newDllPath = AppKnownFolders.Extensions.CombinePath(dllName);
                                    if (ExtensionService.TryLoadHost(newDllPath))
                                    {
                                        this.SuccessGrowl(ExtensionsPageResources.DllLoadedSuccessfully, fileName);
                                        continue;
                                    }

                                    this.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, fileName);
                                }
                                catch (Exception ex)
                                {
                                    this.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, $"{fileName}: {ex.Message}");
                                }
                            }
                        }
                        else
                            this.WarningGrowl(ExtensionsPageResources.ZipContainsNoDll, fileName);
                    }
                    catch (Exception ex)
                    {
                        this.ErrorGrowl(ExtensionsPageResources.UnzipFailed, $"{fileName}: {ex.Message}");
                    }

                    break;
            }
        }
    }

    private void UnloadHostOnClick(object sender, RoutedEventArgs e)
    {
        ExtensionService.UnloadHost(sender.To<FrameworkElement>().GetTag<ExtensionsHostModel>());
    }

    private void OpenExtensionsOnClick(object sender, RoutedEventArgs e) => _ = Launcher.LaunchFolderPathAsync(AppKnownFolders.Extensions.FullPath);

    private void ExtensionsHelpOnClick(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(sender.To<FrameworkElement>().GetTag<Uri>());
}
