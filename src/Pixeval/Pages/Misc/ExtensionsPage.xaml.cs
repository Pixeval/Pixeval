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
        var files = await HWnd.OpenMultipleDllsOpenPickerAsync();
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
                        HWnd.ErrorGrowl(ExtensionsPageResources.DllFileExistedError, fileName);
                        continue;
                    }

                    try
                    {
                        fileInfo.CopyTo(newDllPath);
                        if (ExtensionService.TryLoadHost(newDllPath))
                        {
                            HWnd.SuccessGrowl(ExtensionsPageResources.DllLoadedSuccessfully, fileName);
                            continue;
                        }

                        HWnd.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, fileName);
                    }
                    catch (Exception ex)
                    {
                        HWnd.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, $"{fileName}: {ex.Message}");
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
                                        HWnd.SuccessGrowl(ExtensionsPageResources.DllLoadedSuccessfully, fileName);
                                        continue;
                                    }

                                    HWnd.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, fileName);
                                }
                                catch (Exception ex)
                                {
                                    HWnd.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, $"{fileName}: {ex.Message}");
                                }
                            }
                        }
                        else
                            HWnd.WarningGrowl(ExtensionsPageResources.ZipContainsNoDll, fileName);
                    }
                    catch (Exception ex)
                    {
                        HWnd.ErrorGrowl(ExtensionsPageResources.UnzipFailed, $"{fileName}: {ex.Message}");
                    }

                    break;
            }
        }
    }

    private void UnloadHostOnClick(object sender, RoutedEventArgs e)
    {
        ExtensionService.UnloadHost(sender.To<FrameworkElement>().GetTag<ExtensionsHostModel>());
    }

    private void OpenExtensionsOnClick(object sender, RoutedEventArgs e) => Launcher.LaunchFolderPathAsync(AppKnownFolders.Extensions.FullPath);

    private void ExtensionsHelpOnClick(object sender, RoutedEventArgs e) => Launcher.LaunchUriAsync(sender.To<FrameworkElement>().GetTag<Uri>());
}
