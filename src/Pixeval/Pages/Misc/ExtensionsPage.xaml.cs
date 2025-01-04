using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Extensions;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages.Misc;

public sealed partial class ExtensionsPage
{
    private IReadOnlyList<ExtensionsHostModel> Models =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().ExtensionHosts;

    public ExtensionsPage() => InitializeComponent();

    private async void AddExtensionsOnClick(object sender, RoutedEventArgs e)
    {
        var extensionService = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
        var files = await HWnd.OpenMultipleDllsOpenPickerAsync();
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file.Path);
            var dllFileName = fileInfo.Name;
            var newDllPath = AppKnownFolders.Extensions.CombinePath(dllFileName);
            if (File.Exists(newDllPath))
            {
                HWnd.ErrorGrowl(ExtensionsPageResources.DllFileExistedErrorFormatted.Format(dllFileName));
                continue;
            }

            try
            {
                fileInfo.CopyTo(newDllPath);
                if (extensionService.TryLoadHost(newDllPath))
                {
                    HWnd.SuccessGrowl(ExtensionsPageResources.DllLoadedSuccessfullyFormatted.Format(dllFileName));
                    continue;
                }
            }
            catch
            {
                // ignored
            }

            HWnd.ErrorGrowl(ExtensionsPageResources.DllLoadFailedFormatted.Format(dllFileName));
        }
    }
}
