using System.IO;
using System.Linq;

namespace Pixeval.Util;

public static class FileHelper
{
    public static void CreateParentDirectories(string fullPath)
    {
        var directory = Path.GetDirectoryName(fullPath);
        _ = Directory.CreateDirectory(directory!);
    }

    /// <inheritdoc cref="File.Move(string, string, bool)" />
    public static void Move(string sourceFileName, string destFileName, bool overwrite = false)
    {
        CreateParentDirectories(destFileName);
        File.Move(sourceFileName, destFileName, overwrite);
    }

    /// <inheritdoc cref="File.Copy(string, string, bool)" />
    public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
    {
        CreateParentDirectories(destFileName);
        File.Copy(sourceFileName, destFileName, overwrite);
    }

    public static void DeleteEmptyFolder(string? path)
    {
        if (Directory.Exists(path))
            if (!Directory.EnumerateFileSystemEntries(path).Any())
                Directory.Delete(path);
    }

    public static FileStream OpenAsyncRead(string path, int bufferSize = 4096)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
    }

    public static FileStream OpenAsyncWriteCreateParent(string path, int bufferSize = 4096)
    {
        CreateParentDirectories(path);
        return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
    }

    public static FileStream OpenAsyncWrite(string path, int bufferSize = 4096)
    {
        return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
    }

    public static FileStream CreateAsyncWriteCreateParent(string path, int bufferSize = 4096)
    {
        CreateParentDirectories(path);
        return new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, true);
    }

    public static FileStream CreateAsyncWrite(string path, int bufferSize = 4096)
    {
        return new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, true);
    }

    public static FileStream OpenAsyncRead(this FileInfo info, int bufferSize = 4096)
    {
        return info.Open(new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            BufferSize = bufferSize,
            Options = FileOptions.Asynchronous
        });
    }

    public static FileStream OpenAsyncWrite(this FileInfo info, int bufferSize = 4096)
    {
        return info.Open(new FileStreamOptions
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
            Share = FileShare.None,
            BufferSize = bufferSize,
            Options = FileOptions.Asynchronous
        });
    }
}
