// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.IO;
using Pixeval.Utilities;

namespace Pixeval.Models.Download.Tasks;

internal static class DownloadTaskFileHelper
{
    public static bool ShouldSkipExistingFile(string destination, bool overwrite)
    {
        if (!File.Exists(destination))
            return false;
        if (!overwrite)
            return true;

        File.Delete(destination);
        return false;
    }

    /// <returns><see langword="true" /> if the temporary file became the destination.</returns>
    public static bool CommitDownloadedFile(string temporaryFile, string destination, bool overwrite)
    {
        if (overwrite)
        {
            FileHelper.Move(temporaryFile, destination, true);
            return true;
        }

        try
        {
            FileHelper.Move(temporaryFile, destination);
            return true;
        }
        catch (IOException) when (File.Exists(temporaryFile) && File.Exists(destination))
        {
            File.Delete(temporaryFile);
            return false;
        }
    }
}
