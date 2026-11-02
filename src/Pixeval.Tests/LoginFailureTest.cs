// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mako;
using Mako.Net.EndPoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.AppManagement;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using SQLite;
using WebApiClientCore;

namespace Pixeval.Tests;

[TestClass]
public sealed class LoginFailureTest
{
    [TestMethod]
    [DoNotParallelize]
    public async Task SavedAccounts_DatabaseOpenFailureIsDeferredAndCanBeRetried()
    {
        var unavailableDirectory = Path.GetTempFileName();
        var appProperty = typeof(App).GetProperty(nameof(App.AppViewModel), BindingFlags.Public | BindingFlags.Static)!;
        var previousAppViewModel = appProperty.GetValue(null);
        try
        {
            using var db = new SQLiteConnection(":memory:");
            var manager = new LoginUserPersistentManager(db);
            var user = manager.Upsert(new() { UserId = 1, RefreshToken = "saved-token" });
            var attempts = 0;
            using var services = new ServiceCollection().AddSingleton<LoginUserPersistentManager>(_ =>
            {
                attempts++;
                if (attempts is 1)
                {
                    using var unavailableDb = new SQLiteConnection(Path.Combine(unavailableDirectory, "login.sqlite"));
                }
                return manager;
            }).BuildServiceProvider();
            var appViewModel = (AppViewModel) RuntimeHelpers.GetUninitializedObject(typeof(AppViewModel));
            typeof(AppViewModel).GetProperty(nameof(AppViewModel.AppServiceProvider))!.SetValue(appViewModel, services);
            typeof(AppViewModel).GetField("<LoginContext>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(appViewModel, new LoginContext { CurrentKey = user.HistoryEntryId });
            appProperty.SetValue(null, appViewModel);
            var viewModel = new LoginPageViewModel();

            Assert.AreEqual(0, attempts);
            var exception = await Assert.ThrowsExactlyAsync<SQLiteException>(() => viewModel.LoadUsersAsync());
            Assert.IsTrue(LoginErrorHelper.IsStorageFailure(exception));
            Assert.IsEmpty(viewModel.Users);

            await viewModel.LoadUsersAsync();

            Assert.AreEqual(2, attempts);
            Assert.AreEqual(user.HistoryEntryId, viewModel.SelectedUser?.HistoryEntryId);
            Assert.AreEqual("saved-token", viewModel.RefreshToken);
        }
        finally
        {
            appProperty.SetValue(null, previousAppViewModel);
            File.Delete(unavailableDirectory);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task TokenValidation_PreservesDatabasePersistenceResult(bool readOnly)
    {
        using var db = new SQLiteConnection(":memory:");
        var manager = new LoginUserPersistentManager(db);
        _ = manager.Upsert(new() { UserId = 1, RefreshToken = "old-token" });
        if (readOnly)
            _ = db.Execute("PRAGMA query_only = ON");
        await using var client = CreateClient(HttpStatusCode.OK);
        var tokenReceived = false;
        client.TokenRefreshed += (sender, response) =>
        {
            if (response is null)
                return;
            tokenReceived = true;
            _ = manager.Upsert(LoginUserEntry.FromTokenUser(response.RefreshToken, response.User));
        };
        await client.SetTokenAsync("old-token");

        if (readOnly)
        {
            var exception = await Assert.ThrowsExactlyAsync<SQLiteException>(() => client.ValidateTokenAsync());
            Assert.AreEqual(SQLite3.Result.ReadOnly, exception.Result);
            Assert.IsTrue(LoginErrorHelper.IsStorageFailure(exception));
        }
        else
            await client.ValidateTokenAsync();

        Assert.IsTrue(tokenReceived);
        Assert.AreEqual(readOnly ? "old-token" : "new-token", manager.GetByUserId(1)?.RefreshToken);
    }

    [TestMethod]
    public async Task TokenValidation_AuthenticationFailureReachesCaller()
    {
        await using var client = CreateClient(HttpStatusCode.Unauthorized);
        await client.SetTokenAsync("invalid-token");

        var exception = await Assert.ThrowsAsync<Exception>(() => client.ValidateTokenAsync());

        Assert.IsFalse(LoginErrorHelper.IsStorageFailure(exception));
        Assert.IsNull(client.Me);
    }

    [TestMethod]
    public void NetworkIoFailure_IsNotReportedAsAStorageFailure()
    {
        var exception = new HttpRequestException("Connection failed", new IOException("Connection closed"));

        Assert.IsFalse(LoginErrorHelper.IsStorageFailure(exception));
        Assert.IsTrue(LoginErrorHelper.IsStorageFailure(new IOException("Disk full")));
        Assert.IsTrue(LoginErrorHelper.IsStorageFailure(
            new InvalidOperationException("Cannot save login", new UnauthorizedAccessException("Access denied"))));
    }

    private static MakoClient CreateClient(HttpStatusCode statusCode)
    {
        var client = new MakoClient(new(), NullLogger.Instance);
        var options = client.Provider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>()
            .Get(HttpApi.GetName(typeof(IAuthEndPoint)));
        options.HttpMessageHandlerBuilderActions.Add(builder => builder.PrimaryHandler = new TokenResponseHandler(statusCode));
        return client;
    }

    private sealed class TokenResponseHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                RequestMessage = request,
                Content = new StringContent(
                    """
                    {
                      "access_token": "access-token", "refresh_token": "new-token",
                      "expires_in": 3600, "token_type": "bearer", "scope": "",
                      "user": {
                        "id": 1, "name": "user", "account": "account", "mail_address": "",
                        "is_premium": false, "x_restrict": 0, "is_mail_authorized": true,
                        "require_policy_agreement": false,
                        "profile_image_urls": { "px_16x16": "", "px_50x50": "", "px_170x170": "" }
                      }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });
    }
}
