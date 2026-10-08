using System;
using Avalonia;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using Velopack;

namespace Pixeval.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        StoreDataMigration.TryMigrateFromMicrosoftStore();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new SkiaOptions
            {
                // 提高到256M对于4K分辨率勉强够用，在瀑布流这种有巨量图片同时渲染的场景，应该能大幅提升性能
                // 再往上拉还能有提升，不过必要性不高（？）
                MaxGpuResourceSizeBytes = 256 * 1024 * 1024
            })
            .WithPixevalFonts()
            .LogToTrace();
}
