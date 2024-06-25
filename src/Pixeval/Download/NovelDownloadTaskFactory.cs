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

using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class NovelDownloadTaskFactory : IDownloadTaskFactory<NovelItemViewModel, NovelDownloadTaskGroup>
{
    public IMetaPathParser<NovelItemViewModel> PathParser { get; } = new NovelMetaPathParser();

    public NovelDownloadTaskGroup Create(NovelItemViewModel context, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        path += "\\" + IoHelper.GetIllustrationExtension();
        _ = manager.TryDelete(entry => entry.Destination == path);
        var task = new NovelDownloadTaskGroup(context.Entry, path);
        manager.Insert(task.DatabaseEntry);
        return task;
    }

    public NovelDownloadTaskGroup CreateIntrinsic(NovelItemViewModel context, object param, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        // xxx.pdf\.png
        // xxx.pdf\<ext>
        // xxx\novel.txt\.png
        // xxx\novel.md\<ext>
        path += "\\" + IoHelper.GetIllustrationExtension();
        _ = manager.TryDelete(entry => entry.Destination == path);
        var viewModel = (DocumentViewerViewModel)param;
        var task = new NovelDownloadTaskGroup(context.Entry, viewModel.NovelContent, viewModel, path);
        manager.Insert(task.DatabaseEntry);
        return task;
    }
}
