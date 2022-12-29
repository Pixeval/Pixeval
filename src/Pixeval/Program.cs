using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pixeval.CoreApi;
using Pixeval.Models;
using Pixeval.Pages;
using Pixeval.Services.Navigation;
using Pixeval.Storage;
using Pixeval.ViewModels;
using System;
using System.Globalization;
using System.IO;
using Windows.Storage;
using Pixeval.Navigation;
using Pixeval.Startup.WinUI.Hosting;
using Microsoft.VisualStudio.Threading;
using Pixeval.Data;

namespace Pixeval
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureServices(services =>
            {

                services.ConfigureRoutes<MainPage, MainPageNavigationService>(routesBuilder =>
                {
                    //routesBuilder.AddPageWithRoute<RankingsPage>("/Rankings");
                    //routesBuilder.AddPageWithRoute<BookmarksPage>("/Bookmarks");
                    //routesBuilder.AddPageWithRoute<SpotlightsPage>("/Spotlights");
                    //routesBuilder.AddPageWithRoute<RecentPostsPage>("/RecentPosts");
                    //routesBuilder.AddPageWithRoute<SearchResultsPage>("/SearchResults");
                    //routesBuilder.AddPageWithRoute<RecommendationPage>("/Recommendations");
                    //routesBuilder.AddPageWithRoute<SettingsPage, SettingsPageViewModel>("/Settings");
                    //routesBuilder.AddPageWithRoute<FollowingsPage, FollowingsPageViewModel>("/Followings");
                    //routesBuilder.AddPageWithRoute<DownloadListPage, DownloadListPageViewModel>("/Downloads");
                });

                //services.ConfigureRoutes<IllustrationViewerPage, IllustrationViewerPageNavigationService>(
                //    routesBuilder =>
                //    {
                //        routesBuilder.AddPageWithRoute<CommentsPage>("/Comments");

                //    });

                services.Configure<PixivClientOptions>(options =>
                {
                    options.Bypass = true;
                    options.ConnectionTimeout = 5000;
                    options.CultureInfo = CultureInfo.CurrentCulture;
                });

                services.Configure<PixivHttpOptions>(options =>
                {
                    options.AppApiBaseUrl = "https://app-api.pixiv.net";
                    options.WebApiBaseUrl = "https://www.pixiv.net";
                    options.OAuthBaseUrl = "https://oauth.secure.pixiv.net";
                });

                services.AddPixivApiService(config =>
                {

                });

                services.AddPixivApiSession<SessionRefresher>();

                services.AddSingleton<SessionStorage>();

                services.AddSingleton<SettingStorage>();

                services.AddSingleton<MainWindow>();

                services.AddSingleton<MainPage>();
                services.AddSingleton<MainPageViewModel>();

                services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));

                services.AddSingleton<JoinableTaskFactory>();

                services.AddSingleton<ILiteDatabaseAsync>(
                    new LiteDatabaseAsync(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "data.db")));

                services.AddWinUIApp<App>();
            });
            var app = builder.Build();
            app.Run();
        }
    }
}
