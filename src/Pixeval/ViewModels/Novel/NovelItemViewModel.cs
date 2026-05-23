// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Mako.Model;
using Pixeval.Controls;

namespace Pixeval.ViewModels;

public partial class NovelItemViewModel(Novel novel) : WorkEntryViewModel<Novel>(novel), IFactory<Novel, NovelItemViewModel>
{
    public static NovelItemViewModel CreateInstance(Novel entry) => new(entry);

    public int TextLength => Entry.TextLength;

    public Task<NovelContent> ContentAsync => _contentAsync.Value;

    private readonly Lazy<Task<NovelContent>> _contentAsync =
        new(() => App.AppViewModel.MakoClient.GetNovelContentAsync(novel.Id));
}
