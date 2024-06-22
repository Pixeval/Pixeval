#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelDownloadTaskFactory.cs
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

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;
using Pixeval.Options;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download;

public class NovelDownloadTaskFactory : IDownloadTaskFactory<NovelItemViewModel, NovelDownloadTask>
{
    public IMetaPathParser<NovelItemViewModel> PathParser { get; } = new NovelMetaPathParser();

    public async Task<NovelDownloadTask> CreateAsync(NovelItemViewModel context, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        if (App.AppViewModel.AppSettings.NovelDownloadFormat is not NovelDownloadFormat.Pdf)
            path += IoHelper.GetIllustrationExtension();
        if (manager.Collection.Exists(entry => entry.Destination == path))
        {
            // delete the original entry
            _ = manager.Delete(entry => entry.Destination == path);
        }

        var task = await Functions.Block(async () =>
        {
            var novelContent = await context.GetNovelContentAsync();
            var downloadHistoryEntry = new DownloadHistoryEntry(
                path,
                DownloadItemType.Novel,
                context.Entry);
            return new NovelDownloadTask(downloadHistoryEntry, context, novelContent);
        });

        manager.Insert(task.DatabaseEntry);
        return task;
    }

    public NovelDownloadTask CreateIntrinsic(NovelItemViewModel context, object param, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        // xxx.pdf\.png
        // xxx.pdf\<ext>
        // xxx\novel.txt\.png
        // xxx\novel.md\<ext>
        path += "\\" + IoHelper.GetIllustrationExtension();
        if (manager.Collection.Exists(entry => entry.Destination == path))
        {
            // delete the original entry
            _ = manager.Delete(entry => entry.Destination == path);
        }
        var viewModel = (DocumentViewerViewModel)param;
        var entry = new DownloadHistoryEntry(path, DownloadItemType.Novel, context.Entry);
        return new IntrinsicNovelDownloadTask(entry, context, viewModel);
    }
}
