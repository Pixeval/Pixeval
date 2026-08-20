// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Utilities;
using Pixeval.Utilities.GitHub;
using Velopack;
using Velopack.Logging;
using Velopack.Sources;

namespace Pixeval.AppManagement;

public class Versioning
{
    private const string GitHubRepositoryUri = "https://github.com/Pixeval/Pixeval";

    private GithubSource? _velopackSource;
    private UpdateManager? _velopackUpdateManager;
    private UpdateInfo? _velopackUpdateInfo;
    private AppReleaseModel? _velopackUpdateReleaseModel;
    private readonly SemaphoreSlim _updateCheckLock = new(1, 1);
    private readonly SemaphoreSlim _updateDownloadLock = new(1, 1);
    private bool _updateApplyRequested;

    public Versioning()
    {
        var assembly = typeof(Versioning).Assembly;
        CurrentVersion = assembly.GetName().Version ?? new(0, 0, 0, 0);
        CurrentVersionShortText = CurrentVersion.ToString();
        CurrentVersionFullText = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? CurrentVersionShortText;
    }

    public Version CurrentVersion { get; }

    public string CurrentVersionShortText { get; }

    /// <remarks>
    /// <see cref="AssemblyInformationalVersionAttribute"/> 包含 GitSha 信息
    /// </remarks>
    public string CurrentVersionFullText { get; }

    public Version? NewestVersion => NewestAppReleaseModel?.Version;

    public AppReleaseModel? NewestAppReleaseModel => _velopackUpdateReleaseModel ?? AppReleaseModels?.FirstOrDefault();

    public AppReleaseModel? CurrentAppReleaseModel => AppReleaseModels?.FirstOrDefault(t => t.Version == CurrentVersion);

    public UpdateState CompareUpdateState(Version currentVersion, Version? newVersion)
    {
        if (newVersion is null)
            return UpdateState.Unknown;

        var comparison = currentVersion.CompareTo(newVersion);
        if (comparison is > 0)
            return UpdateState.Insider;
        if (comparison is 0)
            return UpdateState.UpToDate;
        if (currentVersion.Major != newVersion.Major)
            return UpdateState.MajorUpdate;
        if (currentVersion.Minor != newVersion.Minor)
            return UpdateState.MinorUpdate;
        if (currentVersion.Build != newVersion.Build)
            return UpdateState.BuildUpdate;

        // Pixeval 不单独发布 revision 更新，因此将仅 revision 不同的版本归为次要更新。
        return UpdateState.MinorUpdate;
    }

    public UpdateState UpdateState { get; private set; } = UpdateState.Unknown;

    public bool UpdateAvailable => UpdateState is not UpdateState.UpToDate and not UpdateState.Insider and not UpdateState.Unknown;

    public IReadOnlyList<AppReleaseModel>? AppReleaseModels { get; private set; }

    public bool UsesVelopack => StoreDataMigration.IsVelopackInstallation;

    public bool CanApplyUpdate => UsesVelopack && _velopackUpdateManager?.UpdatePendingRestart is not null;

    public Version? PendingUpdateVersion => _velopackUpdateManager?.UpdatePendingRestart?.Version.Version;

    public async Task CheckForUpdateAsync()
    {
        await _updateCheckLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await CheckVelopackForUpdateAsync().ConfigureAwait(false);
        }
        catch
        {
            AppReleaseModels = null;
            _velopackUpdateInfo = null;
            _velopackUpdateReleaseModel = null;
            UpdateState = UpdateState.Unknown;
        }
        finally
        {
            _updateCheckLock.Release();
        }
    }

    public async Task<bool> DownloadUpdateAsync(Action<int>? progress = null, CancellationToken cancelToken = default)
    {
        await _updateDownloadLock.WaitAsync(cancelToken).ConfigureAwait(false);
        try
        {
            if (!UsesVelopack)
                return false;

            var manager = GetVelopackUpdateManager();
            if (manager is null)
                return false;
            if (manager.UpdatePendingRestart is not null)
                return true;
            if (_velopackUpdateInfo is not { } updateInfo)
                return false;

            await manager.DownloadUpdatesAsync(updateInfo, progress ?? (static _ => { }), cancelToken)
                .ConfigureAwait(false);
            return true;
        }
        finally
        {
            _updateDownloadLock.Release();
        }
    }

    public void ApplyUpdateAndRestart()
    {
        if (!UsesVelopack)
            return;

        var manager = GetVelopackUpdateManager();
        var target = manager?.UpdatePendingRestart;
        if (manager is null || target is null)
            return;

        AppInfo.SaveContext();
        _updateApplyRequested = true;
        try
        {
            manager.ApplyUpdatesAndRestart(target, [.. Environment.GetCommandLineArgs().Skip(1)]);
        }
        catch
        {
            _updateApplyRequested = false;
            throw;
        }
    }

    public void ApplyPendingUpdateOnExit()
    {
        if (!UsesVelopack || _updateApplyRequested)
            return;

        try
        {
            var manager = GetVelopackUpdateManager();
            if (manager?.UpdatePendingRestart is not { } target)
                return;

            AppInfo.SaveContext();
            _updateApplyRequested = true;
            manager.WaitExitThenApplyUpdates(target, silent: true, restart: false);
        }
        catch (Exception exception)
        {
            _updateApplyRequested = false;
            App.AppViewModel.AppServiceProvider.GetService<FileLogger>()?.LogError(
                nameof(ApplyPendingUpdateOnExit),
                exception);
        }
    }

    private async Task CheckVelopackForUpdateAsync()
    {
        _velopackUpdateInfo = null;
        _velopackUpdateReleaseModel = null;

        if (!UsesVelopack)
        {
            if (await GetVelopackReleaseModelsAsync().ConfigureAwait(false) is not { Count: > 0 } releaseModels)
            {
                AppReleaseModels = null;
                UpdateState = UpdateState.Unknown;
                return;
            }

            AppReleaseModels = releaseModels;
            UpdateState = CompareUpdateState(CurrentVersion, releaseModels[0].Version);
            App.AppViewModel.AppSettings.ApplicationSettings.LastCheckedUpdate = DateTime.UtcNow;
            return;
        }

        var manager = GetVelopackUpdateManager();
        if (manager is null)
        {
            AppReleaseModels = null;
            UpdateState = UpdateState.Unknown;
            return;
        }

        _velopackUpdateInfo = await manager.CheckForUpdatesAsync().ConfigureAwait(false);
        if (_velopackUpdateInfo is not { TargetFullRelease: { } release })
        {
            _velopackUpdateReleaseModel = null;
            AppReleaseModels = [];
            UpdateState = UpdateState.UpToDate;
        }
        else
        {
            _velopackUpdateReleaseModel = new AppReleaseModel(
                release.Version.Version,
                release.NotesMarkdown ?? string.Empty,
                null);
            AppReleaseModels = [_velopackUpdateReleaseModel];
            UpdateState = CompareUpdateState(CurrentVersion, release.Version.Version);
        }

        App.AppViewModel.AppSettings.ApplicationSettings.LastCheckedUpdate = DateTime.UtcNow;
    }

    public async Task<AppReleaseModel?> GetCurrentAppReleaseModelAsync()
    {
        await _updateCheckLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (AppReleaseModels?.FirstOrDefault(t => t.Version == CurrentVersion) is { } currentRelease)
                return currentRelease;

            if (await GetVelopackReleaseModelsAsync().ConfigureAwait(false) is not { Count: > 0 } appReleaseModels)
                return null;

            AppReleaseModels = appReleaseModels;
            return appReleaseModels.FirstOrDefault(t => t.Version == CurrentVersion);
        }
        catch
        {
            return null;
        }
        finally
        {
            _updateCheckLock.Release();
        }
    }

    private async Task<IReadOnlyList<AppReleaseModel>?> GetVelopackReleaseModelsAsync()
    {
        var feed = await GetVelopackSource()
            .GetReleaseFeed(
                NullVelopackLogger.Instance,
                appId: null,
                channel: VelopackRuntimeInfo.SystemRid)
            .ConfigureAwait(false);

        var appReleaseModels = feed.Assets
            .Where(static asset => asset.Type is VelopackAssetType.Full)
            .GroupBy(static asset => asset.Version.Version)
            .Select(static assets =>
            {
                var release = assets.First();
                return new AppReleaseModel(
                    release.Version.Version,
                    release.NotesMarkdown ?? string.Empty,
                    null);
            })
            .OrderByDescending(static release => release.Version)
            .ToArray();

        return appReleaseModels.Length is 0 ? null : appReleaseModels;
    }

    private GithubSource GetVelopackSource() =>
        _velopackSource ??= new GithubSource(
            GitHubRepositoryUri,
            string.Empty,
            prerelease: false,
            downloader: new GitHubFileDownloader(App.AppViewModel.GetRequiredGitHubUpdateHttpClient));

    private UpdateManager? GetVelopackUpdateManager()
    {
        if (!UsesVelopack)
            return null;

        return _velopackUpdateManager ??= new UpdateManager(
            GetVelopackSource(),
            new UpdateOptions { MaximumDeltasBeforeFallback = 10 });
    }
}

public record AppReleaseModel(
    Version Version,
    string ReleaseNote,
    Uri? ReleaseUri) : IComparable<AppReleaseModel>
{
    public int CompareTo(AppReleaseModel? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (other is null)
            return 1;
        var currentLong =
            ((ulong) Version.Major << 0x30) +
            ((ulong) Version.Minor << 0x20) +
            ((ulong) Version.Build << 0x10) +
            (ulong) Version.Revision;
        var newLong =
            ((ulong) other.Version.Major << 0x30) +
            ((ulong) other.Version.Minor << 0x20) +
            ((ulong) other.Version.Build << 0x10) +
            (ulong) other.Version.Revision;
        if (currentLong > newLong)
            return 1;
        if (currentLong < newLong)
            return -1;
        return 0;
    }
}
