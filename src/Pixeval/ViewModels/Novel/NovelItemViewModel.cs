// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.Models.Blocking;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public partial class NovelItemViewModel(Novel novel)
    : WorkEntryViewModel<Novel>(BlockedContentHelper.Replace(novel)), IFactory<Novel, NovelItemViewModel>
{
    public static NovelItemViewModel CreateInstance(Novel entry) => new(entry);

    public int TextLength => Entry.TextLength;

    public Task<NovelContent> ContentAsync => _contentAsync.Value;

    private readonly Lazy<Task<NovelContent>> _contentAsync =
        new(() => BlockedContentHelper.IsBlockedPlaceholder(novel)
            ? Task.FromResult(BlockedContentModelHelper.CreateBlockedNovelContent(BlockedContentHelper.Replace(novel)))
            : App.AppViewModel.MakoClient.GetNovelContentAsync(novel.Id));
}
