// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Pixeval.Util;

namespace Pixeval.AppManagement;

public class AppKnownFolders(string fullPath)
{
    public static AppKnownFolders Temp { get; } = new(AppInfo.AppData.TemporaryFolder.Path);

    public static AppKnownFolders Local { get; } = new(AppInfo.AppData.LocalFolder.Path);

    public static AppKnownFolders Cache { get; } = new(AppInfo.AppData.LocalCacheFolder.Path);

    public static AppKnownFolders Logs { get; } = new(Local.FullPath, nameof(Logs));

    public static AppKnownFolders Settings { get; } = new(Local.DirectoryInfo.Parent?.FullName ?? "", nameof(Settings));

    public static AppKnownFolders Wallpapers { get; } = new(Local.FullPath, nameof(Wallpapers));

    public static AppKnownFolders Extensions { get; } = new(Local.FullPath, nameof(Extensions));

    public static FileInfo SettingsDat { get; } = new FileInfo(Path.Combine(Settings.FullPath, SettingsDatName));

    public const string SettingsDatName = "settings.dat";

    private AppKnownFolders(string path, string name) : this(Path.Combine(path, name))
    {
    }

    public void EnsureExisted()
    {
        if (!DirectoryInfo.Exists)
            DirectoryInfo.Create();
    }

    public async Task<StorageFolder> StorageFolderAsync()
    {
        EnsureExisted();
        return await StorageFolder.GetFolderFromPathAsync(FullPath);
    }

    public string FullPath { get; } = fullPath;

    public Uri FullUri { get; } = new(fullPath);

    public DirectoryInfo DirectoryInfo { get; } = Directory.CreateDirectory(new(fullPath));

    public FileStream OpenAsyncRead(string name)
    {
        EnsureExisted();
        return FileHelper.OpenAsyncRead(CombinePath(name));
    }

    public FileStream CreateAsyncWrite(string name)
    {
        EnsureExisted();
        return FileHelper.CreateAsyncWrite(CombinePath(name));
    }

    public void RenameFile(string oldName, string newName)
    {
        EnsureExisted();
        File.Move(CombinePath(oldName), CombinePath(newName));
    }

    public string[] GetFiles(string searchPattern = "*") => Directory.GetFiles(FullPath, searchPattern);

    public string CombinePath(string path) => Path.Combine(FullPath, path);

    public string CombinePath(string path1, string path2) => Path.Combine(FullPath, path1, path2);

    public string CombinePath(params IEnumerable<string> paths) => Path.Combine([FullPath, .. paths]);

    public void Clear()
    {
        if (FullPath == Local.FullPath)
            return;
        if (!DirectoryInfo.Exists)
            return;
        DirectoryInfo.Delete(true);
        DirectoryInfo.Create();
    }
}
