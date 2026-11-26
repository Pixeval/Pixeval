using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Mako;
using Mako.Global.Enum;
using Mako.Net.EndPoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.AppManagement.Settings;
using Pixeval.I18N;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Entry;
using Pixeval.Views.Work;
using SQLite;
using WebApiClientCore;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class WorkViewContextMenuTest
{
    [TestMethod]
    [DataRow(RawInputModifiers.Control)]
    [DataRow(RawInputModifiers.Shift)]
    public async Task ModifierClickStartsMultiSelectionAndContextClickUsesCurrentSelection(RawInputModifiers modifier)
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(IllustrationItemLayoutTest.IllustrationTestApplication));
        await session.Dispatch(async () =>
        {
            using var db = new SQLiteConnection(":memory:");
            await using var client = new MakoClient(new(), NullLogger.Instance);
            var requests = new List<string>();
            var httpOptions = client.Provider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();
            var appApi = typeof(MakoClient).Assembly.GetType("Mako.Net.EndPoints.IAppApiEndPoint", throwOnError: true)!;
            foreach (var endpoint in new[] { typeof(IAuthEndPoint), appApi })
                httpOptions.Get(HttpApi.GetName(endpoint)).HttpMessageHandlerBuilderActions.Add(
                    builder => builder.PrimaryHandler = new TestPixivHandler(requests));
            await client.SetTokenAsync("test-refresh-token");
            using var services = new ServiceCollection()
                .AddSingleton(new BlockedUserPersistentManager(db))
                .AddKeyedSingleton<IGetArtworkService>(IPlatformInfo.Pixiv, client)
                .AddKeyedSingleton<IDownloadHttpClientService>(IPlatformInfo.Pixiv, client)
                .BuildServiceProvider();
            var appModel = (AppViewModel) RuntimeHelpers.GetUninitializedObject(typeof(AppViewModel));
            typeof(AppViewModel).GetProperty(nameof(AppViewModel.AppServiceProvider))!.SetValue(appModel, services);
            typeof(AppViewModel).GetField("<AppSettings>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(appModel, new AppSettings { ApplicationSettings = new() { UseFileCache = false } });
            var appProperty = typeof(App).GetProperty(nameof(App.AppViewModel))!;
            var previousApp = App.AppViewModel;
            var illustration = DesignHelper.DesignIllustration;
            Application.Current!.Resources["ControlOnImageFillColorDefaultBrush"] = Brushes.Transparent;
            var window = new Window { Width = 1200, Height = 700 };
            appProperty.SetValue(null, appModel);
            try
            {
                var container = new WorkContainer();
                using var view = container.WorkView;
                view.LayoutType = ThumbnailLayoutType.Grid;
                view.FindControl<EntryView>("EntryView")!.Template = new FuncControlTemplate<EntryView>(
                    (control, _) => new ContentPresenter { Content = control.Content });
                var list = view.WorkListBox;
                var selectionMenu = list.ContextMenu!;
                window.Content = container;
                view.SetSource(
                [
                    illustration with { Id = 1, IsFavorite = false, User = illustration.User with { Id = 10, IsFollowed = false } },
                    illustration with { Id = 2, IsFavorite = true, User = illustration.User with { Id = 10, IsFollowed = false } },
                    illustration with { Id = 3, IsFavorite = false, User = illustration.User with { Id = 20, IsFollowed = true } },
                    illustration with { Id = 4, User = illustration.User with { Id = 30, IsFollowed = false } }
                ], SimpleWorkType.Illustration);
                var model = (IOperableViewViewModel) view.DataContext!;
                window.Show();
                UpdateLayout();

                list.SelectedIndex = 1;
                Assert.IsFalse(model.IsSelecting);
                _ = ClickItem(0, MouseButton.Left, modifier);
                Assert.IsTrue(model.IsSelecting);
                Assert.IsTrue(container.SelectionToggleButton.IsChecked is true);
                Assert.IsTrue(container.SelectionToolBar.IsVisible);
                var toolbarButtons = container.SelectionToolBar.PrimaryCommands.OfType<CommandBarButton>().ToArray();
                Assert.AreSame(view.SelectionMenu.BookmarkAllCommand, container.AddAllToBookmarkButton.Command);
                Assert.AreSame(view.SelectionMenu.SaveCommand,
                    toolbarButtons.Single(button => button.Icon is SymbolIcon { Symbol: Symbol.Save }).Command);
                Assert.AreSame(view.SelectionMenu.OpenInBrowserCommand,
                    toolbarButtons.Single(button => button.Icon is SymbolIcon { Symbol: Symbol.GlobeArrowUp }).Command);
                CollectionAssert.AreEqual(new[] { 0 }, list.Selection.SelectedIndexes.ToArray());
                _ = ClickItem(2, MouseButton.Left, RawInputModifiers.Shift);
                CollectionAssert.AreEqual(new[] { 0, 1, 2 }, list.Selection.SelectedIndexes.ToArray());

                var selectedItemMenu = ClickItem(0, MouseButton.Right);
                Assert.IsFalse(selectedItemMenu.IsOpen);
                Assert.IsTrue(selectionMenu.IsOpen);
                CollectionAssert.AreEqual(new[] { 0, 1, 2 }, list.Selection.SelectedIndexes.ToArray());
                var headers = selectionMenu.Items.OfType<MenuItem>().Select(item => item.Header).ToArray();
                CollectionAssert.Contains(headers, I18NManager.GetResource(WorkContainerResources.SelectionMenu.SaveSelectedWorks));
                CollectionAssert.Contains(headers, I18NManager.GetResource(WorkContainerResources.SelectionMenu.OpenSelectedWorksInBrowser));
                CollectionAssert.DoesNotContain(headers, I18NManager.GetResource(EntryItemResources.Copy));
                CollectionAssert.DoesNotContain(headers, I18NManager.GetResource(EntryItemResources.GenerateWebLink));

                var addBookmark = FindCountedAction(WorkContainerResources.SelectionMenu.BookmarkWorksFormatted, 2);
                var removeBookmark = FindCountedAction(WorkContainerResources.SelectionMenu.RemoveBookmarksFormatted, 1);
                var follow = FindCountedAction(WorkContainerResources.SelectionMenu.FollowAuthorsFormatted, 1);
                var unfollow = FindCountedAction(WorkContainerResources.SelectionMenu.UnfollowAuthorsFormatted, 1);
                var watchLater = FindAction(selectionMenu, I18NManager.GetResource(WorkContainerResources.SelectionMenu.AddSelectedToWatchLater));
                var blockAuthors = FindAction(selectionMenu, I18NManager.GetResource(WorkContainerResources.SelectionMenu.BlockSelectedAuthors));
                var separator = selectionMenu.Items.IndexOf(selectionMenu.Items.OfType<Separator>().Single());
                Assert.IsTrue(selectionMenu.Items.IndexOf(removeBookmark) < separator);
                Assert.IsTrue(selectionMenu.Items.IndexOf(watchLater) < separator);
                Assert.IsTrue(selectionMenu.Items.IndexOf(follow) > separator);
                Assert.IsTrue(selectionMenu.Items.IndexOf(blockAuthors) > separator);
                list.Selection.Deselect(2);
                await ExecuteAsync(addBookmark);
                Assert.IsFalse(model.Source.Single(work => work.Entry.Id is "3").Entry.IsFavorite);
                list.Selection.Select(2);
                await ExecuteAsync(addBookmark);
                await ExecuteAsync(unfollow);
                Assert.IsTrue(model.SelectedEntries.All(work => work.Entry.IsFavorite));
                selectionMenu.Close();
                UpdateLayout();

                _ = ClickItem(0, MouseButton.Right);
                Assert.AreSame(addBookmark, FindCountedAction(WorkContainerResources.SelectionMenu.BookmarkWorksFormatted, 0));
                Assert.AreSame(unfollow, FindCountedAction(WorkContainerResources.SelectionMenu.UnfollowAuthorsFormatted, 0));
                await ExecuteAsync(FindCountedAction(WorkContainerResources.SelectionMenu.RemoveBookmarksFormatted, 3));
                await ExecuteAsync(FindCountedAction(WorkContainerResources.SelectionMenu.FollowAuthorsFormatted, 2));
                Assert.IsTrue(model.SelectedEntries.All(work => !work.Entry.IsFavorite));
                selectionMenu.Close();
                UpdateLayout();

                _ = ClickItem(0, MouseButton.Right);
                _ = FindCountedAction(WorkContainerResources.SelectionMenu.RemoveBookmarksFormatted, 0);
                _ = FindCountedAction(WorkContainerResources.SelectionMenu.FollowAuthorsFormatted, 0);
                selectionMenu.Close();
                UpdateLayout();

                var singleMenu = ClickItem(3, MouseButton.Right);
                Assert.IsTrue(singleMenu.IsOpen);
                Assert.IsFalse(selectionMenu.IsOpen);
                CollectionAssert.AreEqual(new[] { 3 }, list.Selection.SelectedIndexes.ToArray());
                var followItem = singleMenu.Items.OfType<MenuItem>().Single(
                    menuItem => Equals(menuItem.Header, I18NManager.GetResource(EntryItemResources.FollowAuthor)));
                Assert.IsTrue(followItem.IsVisible);
                Assert.IsTrue(followItem.IsEffectivelyEnabled);
                var followIcon = (SymbolIcon) followItem.Icon!;
                Assert.AreEqual(Symbol.PersonAdd, followIcon.Symbol);
                Assert.AreEqual(IconVariant.Regular, followIcon.IconVariant);
                Assert.IsTrue(singleMenu.Items.OfType<MenuItem>().Any(
                    menuItem => Equals(menuItem.Header, I18NManager.GetResource(EntryItemResources.GenerateWebLink))));
                var singleBookmark = FindAction(singleMenu, I18NManager.GetResource(EntryItemResources.AddToBookmark));
                Assert.AreEqual(IconVariant.Color, ((SymbolIcon) singleBookmark.Icon!).IconVariant);
                await ExecuteAsync(followItem);
                Assert.AreEqual(IconVariant.Color, followIcon.IconVariant);
                await ExecuteAsync(singleBookmark);
                Assert.AreEqual(IconVariant.Regular, ((SymbolIcon) singleBookmark.Icon!).IconVariant);
                singleMenu.Close();
                UpdateLayout();

                var reopenedMenu = ClickItem(3, MouseButton.Right);
                Assert.AreEqual(IconVariant.Color, followIcon.IconVariant);
                Assert.IsTrue(followItem.IsEffectivelyEnabled);
                await ExecuteAsync(followItem);
                await ExecuteAsync(singleBookmark);
                Assert.AreEqual(IconVariant.Color, ((SymbolIcon) singleBookmark.Icon!).IconVariant);
                reopenedMenu.Close();
                UpdateLayout();

                model.IsSelecting = false;
                Assert.IsTrue(container.SelectionToggleButton.IsChecked is false);
                var followedAuthorMenu = ClickItem(2, MouseButton.Right);
                Assert.IsTrue(followedAuthorMenu.IsOpen);
                var followedAuthorItem = followedAuthorMenu.Items.OfType<MenuItem>().Single(
                    menuItem => Equals(menuItem.Header, I18NManager.GetResource(EntryItemResources.FollowAuthor)));
                Assert.IsTrue(followedAuthorItem.IsEffectivelyEnabled);
                Assert.AreEqual(IconVariant.Color, ((SymbolIcon) followedAuthorItem.Icon!).IconVariant);
                followedAuthorMenu.Close();

                CollectionAssert.AreEqual(new[]
                {
                    "/v2/illust/bookmark/add:1", "/v2/illust/bookmark/add:3", "/v1/user/follow/delete:20",
                    "/v1/illust/bookmark/delete:1", "/v1/illust/bookmark/delete:2", "/v1/illust/bookmark/delete:3",
                    "/v1/user/follow/add:10", "/v1/user/follow/add:20", "/v1/user/follow/add:30",
                    "/v1/illust/bookmark/delete:4", "/v1/user/follow/delete:30", "/v2/illust/bookmark/add:4"
                }, requests);

                MenuItem FindCountedAction(string resource, int count)
                {
                    var item = FindAction(selectionMenu, I18NManager.GetResource(resource, count));
                    Assert.AreEqual(count > 0, item.IsEffectivelyEnabled);
                    return item;
                }

                ContextMenu ClickItem(int index, MouseButton button, RawInputModifiers modifiers = RawInputModifiers.None)
                {
                    var item = (ListBoxItem) list.ContainerFromIndex(index)!;
                    var card = (WorkItem) item.Content!;
                    var host = card.GetLogicalDescendants().OfType<Control>().Single(control => control.ContextMenu is not null);
                    var position = host.TranslatePoint(host.Bounds.Center - host.Bounds.Position, window)!.Value;
                    window.MouseMove(position);
                    window.MouseDown(position, button, modifiers);
                    window.MouseUp(position, button, modifiers);
                    UpdateLayout();
                    return host.ContextMenu!;
                }
            }
            finally
            {
                window.Close();
                appProperty.SetValue(null, previousApp);
            }

            void UpdateLayout()
            {
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }
        }, CancellationToken.None);
    }

    private static MenuItem FindAction(ContextMenu menu, string header) =>
        menu.Items.OfType<MenuItem>().Single(menuItem => Equals(menuItem.Header, header));

    private static Task ExecuteAsync(MenuItem item) => ((IAsyncRelayCommand) item.Command!).ExecuteAsync(item.CommandParameter);

    private sealed class TestPixivHandler(List<string> requests) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            if (path is "/auth/token")
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "access_token": "test-access-token", "refresh_token": "test-refresh-token",
                          "expires_in": 3600, "token_type": "bearer", "scope": "",
                          "user": {
                            "id": 100, "name": "test", "account": "test", "mail_address": "",
                            "is_premium": false, "x_restrict": 0, "is_mail_authorized": true,
                            "require_policy_agreement": false,
                            "profile_image_urls": { "px_16x16": "", "px_50x50": "", "px_170x170": "" }
                          }
                        }
                        """, Encoding.UTF8, "application/json")
                };

            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            var id = body.Split('&').Single(field => field.StartsWith("illust_id=", StringComparison.Ordinal)
                || field.StartsWith("user_id=", StringComparison.Ordinal)).Split('=')[1];
            requests.Add($"{path}:{id}");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        }
    }
}
