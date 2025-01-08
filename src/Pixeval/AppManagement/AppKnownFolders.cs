// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Pixeval.Util.IO;

namespace Pixeval.AppManagement;

public class AppKnownFolders(string fullPath)
{
    public static AppKnownFolders Temp { get; } = new(AppInfo.AppData.TemporaryFolder.Path);

    public static AppKnownFolders Local { get; } = new(AppInfo.AppData.LocalFolder.Path);

    public static AppKnownFolders Cache { get; } = new(AppInfo.AppData.LocalCacheFolder.Path);

    public static AppKnownFolders Logs { get; } = new(Local.FullPath, nameof(Logs));

    public static AppKnownFolders Wallpapers { get; } = new(Local.FullPath, nameof(Wallpapers));

    public static AppKnownFolders Extensions { get; } = new(Local.FullPath, nameof(Extensions));

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

    public FileStream OpenAsyncWrite(string name)
    {
        EnsureExisted();
        return IoHelper.OpenAsyncWrite(CombinePath(name));
    }

    public string[] GetFiles(string searchPattern = "*") => Directory.GetFiles(FullPath, searchPattern);

    public string CombinePath(string path) => Path.Combine(FullPath, path);

    public string CombinePath(string path1, string path2) => Path.Combine(FullPath, path1, path2);

    public string CombinePath(params IEnumerable<string> paths) => Path.Combine([FullPath, ..paths]);

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
