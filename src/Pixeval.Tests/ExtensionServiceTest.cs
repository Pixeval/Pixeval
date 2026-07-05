using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.AppManagement;
using Pixeval.Extensions.Common;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;

namespace Pixeval.Tests;

[TestClass]
public sealed class ExtensionServiceTest
{
    [TestMethod]
    public void InstalledExtensionHostsShouldLoadAndExposeMetadata()
    {
        var logger = new FileLogger(Path.Combine(Path.GetTempPath(), nameof(Pixeval), nameof(ExtensionServiceTest)));
        using var service = new ExtensionService(logger, [], [], loadInstalledHosts: false);
        var failures = new List<string>();
        var loadedHosts = 0;
        var outdatedHosts = 0;

        foreach (var host in ExtensionService.EnumerateLocalExtensionHosts(AppInfo.ExtensionsFolder))
        {
            var result = service.TryLoadHostWithResult(
                host.LibraryPath,
                logger,
                out var model,
                out _,
                host.UninstallTargetRelativePath);
            if (result is ExtensionHostLoadResult.OutdatedSdk)
            {
                ++outdatedHosts;
                continue;
            }

            if (result is not ExtensionHostLoadResult.Loaded || model is null)
            {
                failures.Add($"{Path.GetFileName(host.LibraryPath)}: {result}");
                continue;
            }

            ++loadedHosts;
            AssertHostMetadata(model, host.LibraryPath);
            foreach (var extension in model.Extensions)
                AssertExtensionMetadata(extension, host.LibraryPath);
        }

        if (failures.Count > 0)
            Assert.Fail("Some extension hosts failed to load:" + Environment.NewLine +
                        string.Join(Environment.NewLine, failures));

        if (loadedHosts is 0)
            Assert.Inconclusive(outdatedHosts is 0
                ? $"No extension host native libraries were found in {AppInfo.ExtensionsFolder}."
                : $"Only outdated extension host native libraries were found in {AppInfo.ExtensionsFolder}.");
    }

    private static void AssertHostMetadata(ExtensionsHostModel model, string dll)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(model.Name), $"{dll} host name is empty.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(model.Author), $"{model.Name} author is empty.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(model.Version), $"{model.Name} version is empty.");
        Assert.IsNotNull(model.Description, $"{model.Name} description is null.");
        Assert.IsNotNull(model.Extensions, $"{model.Name} extensions collection is null.");
        var icon = model.Host.GetIcon(out var iconCount);
        Assert.AreEqual(icon?.Length ?? 0, iconCount,
            $"{model.Name} icon count does not match the returned buffer length.");
        Assert.IsNotNull(model.Icon, $"{model.Name} icon control is null.");
        _ = model.Link?.ToString();
        _ = model.HelpLink?.ToString();
    }

    private static void AssertExtensionMetadata(IExtension extension, string dll)
    {
        Assert.IsNotNull(extension, $"{dll} returned a null extension.");

        switch (extension)
        {
            case IEntryExtension entry:
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Label), $"{dll} extension label is empty.");
                Assert.IsNotNull(entry.Description, $"{entry.Label} description is null.");
                break;
            default:
                Assert.IsNotNull(extension.GetType().FullName, $"{dll} extension type name is null.");
                break;
        }
    }
}
