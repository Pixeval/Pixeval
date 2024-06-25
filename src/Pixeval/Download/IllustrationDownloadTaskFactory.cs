#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationDownloadTaskFactory.cs
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

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Models;
using Pixeval.Util.IO;

namespace Pixeval.Download;

public class IllustrationDownloadTaskFactory : IDownloadTaskFactory<IllustrationItemViewModel, IImageDownloadTaskGroup>
{
    public IMetaPathParser<IllustrationItemViewModel> PathParser { get; } = new IllustrationMetaPathParser();

    public IImageDownloadTaskGroup Create(IllustrationItemViewModel context, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        if (manager.Collection.Exists(entry => entry.Destination == path))
        {
            // delete the original entry
            _ = manager.Delete(entry => entry.Destination == path);
        }

        var task = context switch
        {
            { IsUgoira: true } => new UgoiraDownloadTaskGroup(context.Entry, path),
            { IsManga: true, MangaIndex: -1 } => new MangaDownloadTaskGroup(context.Entry, path),
            _ => (IImageDownloadTaskGroup)new SingleImageDownloadTaskGroup(context.Entry, path)
        };

        manager.Insert(task.DatabaseEntry);
        return task;
    }

    public IImageDownloadTaskGroup CreateIntrinsic(IllustrationItemViewModel context, object param, string rawPath)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        var path = IoHelper.NormalizePath(PathParser.Reduce(rawPath, context));
        if (manager.Collection.Exists(entry => entry.Destination == path))
        {
            // delete the original entry
            _ = manager.Delete(entry => entry.Destination == path);
        }

        switch (context)
        {
            case { IsUgoira: true }:
            {
                var (streams, metadata) = ((IReadOnlyList<Stream>, UgoiraMetadataResponse))param;
                return new UgoiraDownloadTaskGroup(context.Entry, metadata, path, streams);
            }
            case { IsManga: true, MangaIndex: -1 }: // 下载一篇漫画（未使用的分支）
            {
                var streams = (IReadOnlyList<Stream>)param;
                return new MangaDownloadTaskGroup(context.Entry, path, streams);
            }
            default:
            {
                var stream = (IReadOnlyList<Stream>)param;
                return new SingleImageDownloadTaskGroup(context.Entry, path, stream[0]);
            }
        }
    }
}
