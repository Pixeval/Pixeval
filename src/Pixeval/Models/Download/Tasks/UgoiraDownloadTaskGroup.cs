// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
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

    private string TempFolderPath { get; set; } = null!;

    private string CsvPath => Path.Combine(TempFolderPath, "intervals in milliseconds.csv");

    private IReadOnlyList<int> MsDelays { get; set; } = null!;

    private void SetTasksSet()
    {
        if (TasksSet.Count > 0)
            return;
        TempFolderPath = DestinationUgoiraFormat.BuiltInFormat is UgoiraDownloadFormat.Original
            ? IoHelper.RemoveTokenExtension(TokenizedDestination)
            : TokenizedDestination + ".tmp";
        var msDelays = new int[Entry.MultiImageUris!.Count];
        for (var i = 0; i < Entry.MultiImageUris.Count; ++i)
        {
            var (uri, msDelay) = Entry.MultiImageUris[i];
            msDelays[i] = msDelay;
            var imageDownloadTask = new ImageDownloadTask(uri,
                Path.Combine(TempFolderPath, $"{i}{Path.GetExtension(uri.OriginalString)}"), DatabaseEntry.State);
            AddToTasksSet(imageDownloadTask);
        }
        MsDelays = msDelays;
        SetNotCreateFromEntry();
    }

    public UgoiraDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationUgoiraFormat = GetFormatToken(entry);
    }

    public UgoiraDownloadTaskGroup(ISingleAnimatedImage entry, string destination) : base(entry, destination, DownloadItemType.Ugoira)
    {
        if (entry.PreferredAnimatedImageType is not SingleAnimatedImageType.MultiFiles) 
            throw new InvalidOperationException($"{nameof(ISingleAnimatedImage.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.MultiFiles)}");
        if (!Entry.MultiImageUris!.IsPreloaded)
            throw new InvalidOperationException($"{nameof(ISingleAnimatedImage.MultiImageUris)} should be preloaded");
        DestinationUgoiraFormat = IoHelper.GetAvailableUgoiraDownloadFormatToken();
        DatabaseEntry.FormatToken = DestinationUgoiraFormat.Value;
    }

    public override ValueTask InitializeTaskGroupAsync()
    {
        SetTasksSet();
        return ValueTask.CompletedTask;
    }

    protected override async Task AfterAllDownloadAsyncOverride(DownloadTaskGroup sender, CancellationToken token = default)
    {
        if (DestinationUgoiraFormat.ExtensionFormatExtension is { } extension)
        {
            await FormatByExtensionAsync(extension);
            return;
        }

        var builtInFormat = DestinationUgoiraFormat.BuiltInFormat ?? UgoiraDownloadFormatToken.DefaultBuiltInFormat;
        if (builtInFormat is not UgoiraDownloadFormat.Original)
            throw new NotSupportedException(builtInFormat.ToString());

        await File.WriteAllTextAsync(CsvPath,
            string.Join(',', MsDelays.Select(t => t.ToString())), token);
    }

    private UgoiraDownloadFormatToken DestinationUgoiraFormat { get; }

    public override string OpenLocalDestination => DestinationUgoiraFormat.BuiltInFormat is UgoiraDownloadFormat.Original ? TempFolderPath : TokenizedDestination;

    public override void Delete()
    {
        foreach (var task in TasksSet)
            task.Delete();
        if (DestinationUgoiraFormat.BuiltInFormat is UgoiraDownloadFormat.Original)
        {
            if (File.Exists(CsvPath))
                File.Delete(CsvPath);
        }
        else if (File.Exists(TokenizedDestination))
            File.Delete(TokenizedDestination);

        FileHelper.DeleteEmptyFolder(TempFolderPath);
    }

    private async Task FormatByExtensionAsync(string extension)
    {
        var provider = GetExtensionService().GetAnimatedImageFormatProvider(extension)
            ?? throw new NotSupportedException(extension);
        var streams = new List<Stream>(TasksSet.Count);
        try
        {
            foreach (var task in TasksSet)
                streams.Add(File.OpenAsyncRead(task.Destination));

            await provider.FormatImageAsync(streams, MsDelays, TokenizedDestination);
        }
        finally
        {
            foreach (var stream in streams)
                await stream.DisposeAsync();
        }

        foreach (var imageDownloadTask in TasksSet)
            imageDownloadTask.Delete();
        FileHelper.DeleteEmptyFolder(TempFolderPath);
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
