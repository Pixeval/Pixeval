// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Download;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Database;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download.Tasks;

/// <summary>
/// 只有<see cref="SingleAnimatedImageType.MultiFiles"/>使用这个类，其他使用<see cref="SingleImageDownloadTaskGroupBase"/>
/// </summary>
public class UgoiraDownloadTaskGroup : DownloadTaskGroup
{
    public ISingleAnimatedImage Entry => (ISingleAnimatedImage) DatabaseEntry.Entry;

    /// <summary>
    /// 表示将每张图下载到的目标文件夹。
    /// 其中如果是原始格式（不打包），则表示目标路径的文件夹，否则表示临时文件夹
    /// </summary>
    private string FolderPath { get; }

    /// <summary>
    /// 非原始格式时的目标文件
    /// </summary>
    private string DestinationFile { get; } = null!;

    /// <summary>
    /// 原始格式时，记录图片间隔的文件
    /// </summary>
    private string CsvFile => Path.Combine(FolderPath, "intervals in milliseconds.csv");

    private IReadOnlyList<int> MsDelays { get; set; } = null!;

    private bool OverwriteDownloadedFile { get; set; }

    private bool SkipFinalOutput { get; set; }

    private void SetTasksSet()
    {
        if (TasksSet.Count > 0)
            return;
        SkipFinalOutput = false;
        OverwriteDownloadedFile = App.AppViewModel.AppSettings.DownloadSettings.OverwriteDownloadedFile;
        if (DatabaseEntry.State is DownloadState.Queued
            && DestinationUgoiraFormat.IsExtension
            && !OverwriteDownloadedFile
            && File.Exists(DestinationFile))
        {
            SkipFinalOutput = true;
            SetNotCreateFromEntry();
            return;
        }

        var msDelays = new int[Entry.MultiImageUris!.Count];
        for (var i = 0; i < Entry.MultiImageUris.Count; ++i)
        {
            var (uri, msDelay) = Entry.MultiImageUris[i];
            msDelays[i] = msDelay;
            var imageDownloadTask = new ImageDownloadTask(uri,
                Path.Combine(FolderPath, $"{i}{Path.GetExtension(uri.OriginalString)}"), DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }
        MsDelays = msDelays;
        SetNotCreateFromEntry();
    }

    public UgoiraDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationUgoiraFormat = GetFormatToken(entry);

        // --- 设置只读路径
        if (DestinationUgoiraFormat.BuiltInFormat is UgoiraDownloadFormat.Original
            || IoHelper.GetUgoiraExtension(DestinationUgoiraFormat) is not { } extension)
            FolderPath = IoHelper.RemoveTokenExtension(TokenizedDestination);
        else
        {
            DestinationFile = IoHelper.ChangeExtension(TokenizedDestination, extension);
            FolderPath = DestinationFile + IoHelper.PixevalTempExtension;
        }
        // ---
    }

    public UgoiraDownloadTaskGroup(ISingleAnimatedImage entry, string destination) : base(entry, destination, DownloadItemType.Ugoira)
    {
        if (entry.PreferredAnimatedImageType is not SingleAnimatedImageType.MultiFiles)
            throw new InvalidOperationException($"{nameof(ISingleAnimatedImage.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.MultiFiles)}");
        if (!Entry.MultiImageUris!.IsPreloaded)
            throw new InvalidOperationException($"{nameof(ISingleAnimatedImage.MultiImageUris)} should be preloaded");
        DestinationUgoiraFormat = IoHelper.GetAvailableUgoiraDownloadFormatToken();
        DatabaseEntry.FormatToken = DestinationUgoiraFormat.Value;

        // --- 设置只读路径
        if (DestinationUgoiraFormat.BuiltInFormat is UgoiraDownloadFormat.Original
            || IoHelper.GetUgoiraExtension(DestinationUgoiraFormat) is not { } extension)
            FolderPath = IoHelper.RemoveTokenExtension(TokenizedDestination);
        else
        {
            DestinationFile = IoHelper.ChangeExtension(TokenizedDestination, extension);
            FolderPath = DestinationFile + IoHelper.PixevalTempExtension;
        }
        // ---
    }

    public override async ValueTask InitializeTaskGroupAsync()
    {
        SetTasksSet();
        if (DatabaseEntry.State is DownloadState.Queued && TasksSet.Count is 0)
            await AllTasksDownloadedAsync();
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        OverwriteDownloadedFile = App.AppViewModel.AppSettings.DownloadSettings.OverwriteDownloadedFile;
        if (SkipFinalOutput)
            return;

        if (DestinationUgoiraFormat.ExtensionFormatExtension is { } extension)
        {
            await FormatByExtensionAsync(extension);
            return;
        }

        var builtInFormat = DestinationUgoiraFormat.BuiltInFormat ?? UgoiraDownloadFormatToken.DefaultBuiltInFormat;
        if (builtInFormat is not UgoiraDownloadFormat.Original)
            throw new NotSupportedException(builtInFormat.ToString());

        if (DownloadTaskFileHelper.ShouldSkipExistingFile(CsvFile, OverwriteDownloadedFile))
            return;

        var temporaryFile = CsvFile + IoHelper.PixevalTempExtension;
        if (File.Exists(temporaryFile))
            File.Delete(temporaryFile);
        try
        {
            FileHelper.CreateParentDirectory(temporaryFile);
            await File.WriteAllTextAsync(temporaryFile,
                string.Join(',', MsDelays.Select(t => t.ToString())), token);
            _ = DownloadTaskFileHelper.CommitDownloadedFile(
                temporaryFile,
                CsvFile,
                OverwriteDownloadedFile);
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }

    private UgoiraDownloadFormatToken DestinationUgoiraFormat { get; }

    public override string OpenLocalDestination => DestinationUgoiraFormat.BuiltInFormat is UgoiraDownloadFormat.Original ? FolderPath : DestinationFile;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (DestinationUgoiraFormat.BuiltInFormat is UgoiraDownloadFormat.Original)
        {
            if (File.Exists(CsvFile))
                File.Delete(CsvFile);
        }
        else if (File.Exists(DestinationFile))
            File.Delete(DestinationFile);

        FileHelper.DeleteEmptyFolder(FolderPath);
    }

    private async Task FormatByExtensionAsync(string extension)
    {
        if (DownloadTaskFileHelper.ShouldSkipExistingFile(DestinationFile, OverwriteDownloadedFile))
        {
            foreach (var task in TasksSet)
                task.Delete();
            FileHelper.DeleteEmptyFolder(FolderPath);
            return;
        }

        var provider = GetExtensionService().GetAnimatedImageFormatProvider(extension)
            ?? throw new NotSupportedException(extension);
        var streams = new List<Stream>(TasksSet.Count);
        var temporaryFile = Path.Combine(FolderPath, Path.GetFileName(DestinationFile));
        if (File.Exists(temporaryFile))
            File.Delete(temporaryFile);
        try
        {
            foreach (var task in TasksSet)
                streams.Add(File.OpenAsyncRead(task.Destination));

            var dict = streams.Zip(MsDelays).ToDictionary(t => t.First, t => t.Second);

            await provider.FormatImageAsync(dict, temporaryFile);
            _ = DownloadTaskFileHelper.CommitDownloadedFile(
                temporaryFile,
                DestinationFile,
                OverwriteDownloadedFile);
        }
        finally
        {
            foreach (var stream in streams)
                await stream.DisposeAsync();
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }

        foreach (var imageDownloadTask in TasksSet)
            imageDownloadTask.Delete();
        FileHelper.DeleteEmptyFolder(FolderPath);
    }

    private static UgoiraDownloadFormatToken GetFormatToken(DownloadHistoryEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.FormatToken))
            return IoHelper.GetAvailableUgoiraDownloadFormatToken(entry.FormatToken);

        return UgoiraDownloadFormatToken.Default;
    }

    private static ExtensionService GetExtensionService() =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}
