#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AppKnownFolders.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StorageFolder = Windows.Storage.StorageFolder;
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
    }
}
