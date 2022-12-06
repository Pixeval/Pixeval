using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pixeval.CoreApi;
using Pixeval.Data;
using Pixeval.Models;
using Pixeval.Pages;
using Pixeval.Pages.Login;
using Pixeval.Startup.WinUI.Hosting;
using Pixeval.Startup.WinUI.Navigation;
using Pixeval.ViewModels;
using System;
using System.Globalization;
using System.IO;
using Windows.Storage;
using Pixeval.Services.Navigation;
using Pixeval.Storage;

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

                services.AddScoped<IBaseRepository<DownloadHistory>, DownloadHistoryRepository>();
                services.AddScoped<IBaseRepository<SearchHistory>, SearchHistoryRepository>();
                services.AddScoped<IBaseRepository<BrowseHistory>, BrowseHistoryRepository>();
                services.AddScoped<IBaseRepository<UserSession>, UserSessionRepository>();
                services.AddScoped<IBaseRepository<UserSetting>, UserSettingRepository>();

                services.AddSingleton<ILiteDatabaseAsync>(new LiteDatabaseAsync(Path.Combine(ApplicationData.Current.LocalFolder.Path, "data.db")));

                services.AddWinUIApp<App>();
            });
            var app = builder.Build();
            app.Run();
        }
    }
}
