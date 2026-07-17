using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixeval.Utilities;

public static class FileHelper
{
    public static IEnumerable<string> EnumerateFiles(
        string directory,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(directory))
            return [];

        try
        {
            return Directory.GetFiles(directory, searchPattern, searchOption);
        }
        catch
        {
            return [];
        }
    }

    public static IEnumerable<string> EnumerateDirectories(
        string directory,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(directory))
            return [];

        try
        {
            return Directory.GetDirectories(directory, searchPattern, searchOption);
        }
        catch
        {
            return [];
        }
    }

    public static bool TryCreateDirectory(string path)
    {
        try
        {
            _ = Directory.CreateDirectory(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void CreateParentDirectory(string fullPath)
    {
        if (Path.GetDirectoryName(fullPath) is { } directory)
            _ = Directory.CreateDirectory(directory);
    }

    /// <inheritdoc cref="File.Move(string, string, bool)" />
    public static void Move(string sourceFileName, string destFileName, bool overwrite = false)
    {
        CreateParentDirectory(destFileName);
        File.Move(sourceFileName, destFileName, overwrite);
    }

    /// <inheritdoc cref="File.Copy(string, string, bool)" />
    public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
    {
        CreateParentDirectory(destFileName);
        File.Copy(sourceFileName, destFileName, overwrite);
    }

    public static void DeleteEmptyFolder(string? path)
    {
        if (Directory.Exists(path))
            if (!Directory.EnumerateFileSystemEntries(path).Any())
                Directory.Delete(path);
    }

    public static FileStream Open(
        string path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        int bufferSize = 4096,
        FileOptions options = FileOptions.None) =>
        new(path, new FileStreamOptions
        {
            Mode = mode,
            Access = access,
            Share = share,
            BufferSize = bufferSize,
            Options = options
        });

    public static FileStream OpenRead(
        string path,
        FileShare share = FileShare.Read,
        int bufferSize = 4096,
        FileOptions options = FileOptions.None) =>
        Open(path, FileMode.Open, FileAccess.Read, share, bufferSize, options);

    public static FileStream OpenWriteCreateParent(
        string path,
        int bufferSize = 4096,
        FileOptions options = FileOptions.None)
    {
        CreateParentDirectory(path);
        return Open(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, options);
    }

    public static FileStream CreateWriteCreateParent(
        string path,
        int bufferSize = 4096,
        FileOptions options = FileOptions.None)
    {
        CreateParentDirectory(path);
        return Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, options);
    }

    public static FileStream OpenAsyncWriteCreateParent(string path, int bufferSize = 4096)
    {
        return OpenWriteCreateParent(path, bufferSize, FileOptions.Asynchronous);
    }

    public static FileStream CreateAsyncWriteCreateParent(string path, int bufferSize = 4096)
    {
        return CreateWriteCreateParent(path, bufferSize, FileOptions.Asynchronous);
    }

    public static void CopyWholeStream(Stream source, Stream destination, int bufferSize = 81920)
    {
        if (!source.CanSeek)
        {
            source.CopyTo(destination, bufferSize);
            return;
        }

        var originalPosition = source.Position;
        try
        {
            source.Position = 0;
            source.CopyTo(destination, bufferSize);
        }
        finally
        {
            source.Position = originalPosition;
        }
    }

    public static bool TryTouchFile(string path) => TryTouchFile(path, DateTime.UtcNow);

    public static bool TryTouchFile(string path, DateTime time)
    {
        try
        {
            File.SetLastWriteTimeUtc(path, time);
            File.SetLastAccessTimeUtc(path, time);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryTouchFileAccessTime(string path)
    {
        try
        {
            File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryDeleteFile(string path) => TryDeleteFile(path, out _);

    public static bool TryDeleteFile(string path, out Exception? exception)
    {
        exception = null;
        try
        {
            File.Delete(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    public static bool TryDeleteDirectory(string path, bool recursive = true) =>
        TryDeleteDirectory(path, recursive, out _);

    public static bool TryDeleteDirectory(string path, bool recursive, out Exception? exception)
    {
        exception = null;
        try
        {
            Directory.Delete(path, recursive);
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    public static bool TryDeleteFileSystemEntry(string path) => TryDeleteFileSystemEntry(path, out _);

    public static bool TryDeleteFileSystemEntry(string path, out Exception? exception)
    {
        if (Directory.Exists(path))
            return TryDeleteDirectory(path, true, out exception);

        return TryDeleteFile(path, out exception);
    }

    extension(File)
    {
        public static FileStream OpenWriteOrTruncate(string path, int bufferSize = 4096)
        {
            var fileMode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;
            return Open(path, fileMode, FileAccess.Write, FileShare.None, bufferSize);
        }

        public static FileStream OpenAsyncRead(string path, int bufferSize = 4096) =>
            OpenRead(path, FileShare.Read, bufferSize, FileOptions.Asynchronous);

        public static FileStream OpenAsyncWriteOrTruncate(string path, int bufferSize = 4096)
        {
            var fileMode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;
            return Open(path, fileMode, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous);
        }

        public static FileStream OpenAsyncWrite(string path, int bufferSize = 4096) =>
            Open(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous);

        public static FileStream CreateAsyncWrite(string path, int bufferSize = 4096) =>
            Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous);
    }

    extension(FileInfo info)
    {
        public FileStream OpenAsyncRead(int bufferSize = 4096) =>
            OpenRead(info.FullName, FileShare.Read, bufferSize, FileOptions.Asynchronous);

        public FileStream OpenAsyncWrite(int bufferSize = 4096) =>
            Open(info.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize,
                FileOptions.Asynchronous);
    }
}
