using System.IO;
using Android.App;
using Android.Content.Res;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Pixeval.I18N;
using Pixeval.Utilities;

namespace Pixeval.Android;

[Application]
public class Application : AvaloniaAndroidApplication<App>
{
    private const string I18nAssetDirectory = "i18n";

    protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        I18NManager.CandidatePaths.Add(ExtractI18nAssets());
        return base.CustomizeAppBuilder(builder)
            .WithPixevalFonts();
    }

    private string ExtractI18nAssets()
    {
        var assets = Assets;
        if (assets.List(I18nAssetDirectory) is not { Length: > 0 })
            throw new DirectoryNotFoundException($"Could not find the bundled {I18nAssetDirectory} assets.");

        var filesDirectory = FilesDir?.AbsolutePath
                             ?? throw new DirectoryNotFoundException("Could not resolve the Android application files directory.");
        var resourceRoot = Path.Combine(filesDirectory, "BundledResources");
        var destinationDirectory = Path.Combine(resourceRoot, I18nAssetDirectory);

        // APK assets are ZIP entries rather than normal files, so materialize them for the directory-based plugin.
        if (Directory.Exists(destinationDirectory))
            Directory.Delete(destinationDirectory, true);

        CopyAssetDirectory(assets, I18nAssetDirectory, destinationDirectory);
        return resourceRoot;
    }

    private static void CopyAssetDirectory(AssetManager assets, string assetDirectory, string destinationDirectory)
    {
        _ = Directory.CreateDirectory(destinationDirectory);
        foreach (var assetName in assets.List(assetDirectory) ?? [])
        {
            var assetPath = $"{assetDirectory}/{assetName}";
            var destinationPath = Path.Combine(destinationDirectory, assetName);
            if (assets.List(assetPath) is { Length: > 0 })
            {
                CopyAssetDirectory(assets, assetPath, destinationPath);
                continue;
            }

            using var source = assets.Open(assetPath);
            using var destination = File.Create(destinationPath);
            source.CopyTo(destination);
        }
    }
}
