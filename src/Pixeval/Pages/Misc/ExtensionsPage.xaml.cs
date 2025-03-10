// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Extensions;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.System;
using WinUI3Utilities;

namespace Pixeval.Pages.Misc;

public sealed partial class ExtensionsPage
{
    private ExtensionService ExtensionService { get; } =
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    private bool _firstTime = true;

    private ObservableCollection<ExtensionsHostModel> Models => ExtensionService.HostModels;

    public ExtensionsPage() => InitializeComponent();

    private void ExtensionsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_firstTime && ExtensionService.OutDateExtensionHostsCount is var count and not 0)
        {
            _firstTime = false;
            this.WarningGrowl(ExtensionsPageResources.SomeExtensionsOutdatedFormatted.Format(count));
        }

        if (App.AppViewModel.VersionContext.NeverUsedExtensions) 
            AppInfo.SaveVersionContext();
    }

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
                        if (ExtensionService.TryLoadHost(newDllPath, out var isOutdated))
                        {
                            this.SuccessGrowl(ExtensionsPageResources.DllLoadedSuccessfully, fileName);
                            continue;
                        }

                        this.ErrorGrowl(
                            isOutdated
                                ? ExtensionsPageResources.ExtensionOutdated
                                : ExtensionsPageResources.DllLoadFailed, fileName);
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
                        var dllNames = await Task.Run(() =>
                        {
                            using var zipArchive = ZipFile.OpenRead(fileInfo.FullName);
                            return zipArchive.Entries.Select(t => t.FullName)
                                .Where(t => !t.Contains('\\') && !t.Contains('/') && t.EndsWith(".dll")).ToArray();
                        });

                        if (dllNames.Length is not 0)
                        {
                            await Task.Run(() =>
                                ZipFile.ExtractToDirectory(fileInfo.FullName, AppKnownFolders.Extensions.FullPath));

                            foreach (var dllName in dllNames)
                                try
                                {
                                    var newDllPath = AppKnownFolders.Extensions.CombinePath(dllName);
                                    if (ExtensionService.TryLoadHost(newDllPath, out var isOutdated))
                                    {
                                        this.SuccessGrowl(ExtensionsPageResources.DllLoadedSuccessfully, fileName);
                                        continue;
                                    }

                                    this.ErrorGrowl(
                                        isOutdated
                                            ? ExtensionsPageResources.ExtensionOutdated
                                            : ExtensionsPageResources.DllLoadFailed, fileName);
                                }
                                catch (Exception ex)
                                {
                                    this.ErrorGrowl(ExtensionsPageResources.DllLoadFailed, $"{fileName}: {ex.Message}");
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

    private void ListViewBase_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs e)
    {
        for (var i = 0; i < Models.Count; ++i)
            Models[i].Priority = i;
    }

    private async void ExtensionCard_OnClick(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri(@"https://github.com/Pixeval/Pixeval/releases"));
}
