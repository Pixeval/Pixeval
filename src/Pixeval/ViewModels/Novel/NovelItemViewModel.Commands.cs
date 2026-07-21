// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.I18N;
using Pixeval.Models.Download;
using Pixeval.Utilities;
using Pixeval.Views.ViewContainers;

namespace Pixeval.ViewModels;

public partial class NovelItemViewModel
{
    protected override Task<bool> SetBookmarkAsync(bool favorite, bool privately = false, IReadOnlyCollection<string>? tags = null)
        => MakoHelper.SetWorkBookmarkAsync(Entry, favorite, privately, tags);

    /// <inheritdoc />
    protected override async Task SaveAsync(Control? parameter)
    {
        SaveInternalAsync(TopLevel.GetTopLevel(parameter)?.ViewContainer, await ContentAsync, App.AppViewModel.AppSettings.DownloadSettings.DownloadPathMacro);
    }

    /// <summary>
    /// <see cref="NovelDownloadTaskFactory"/>
    /// </summary>
    /// <param name="viewContainerBase">承载提示的控件，为<see langword="null"/>则不显示</param>
    /// <param name="content">为<see langword="null"/>则创建新的下载任务</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private void SaveInternalAsync(ViewContainerBase? viewContainerBase, NovelContent content, string path)
    {
        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<NovelDownloadTaskFactory>();
        var task = factory.Create(Entry, path, content);
        App.AppViewModel.HistoryPersistHelper.DownloadManager.QueueTask(task);
        viewContainerBase?.ShowSuccess(I18NManager.GetResource(EntryItemResources.DownloadTaskCreated));
    }

    public override Uri AppUri => Entry.AppUri;

    public override Uri WebsiteUri => Entry.WebsiteUri;
}
