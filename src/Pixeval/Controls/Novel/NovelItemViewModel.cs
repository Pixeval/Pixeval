// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using Mako.Model;

namespace Pixeval.Controls;

public partial class NovelItemViewModel(Novel novel) : WorkEntryViewModel<Novel>(novel), IFactory<Novel, NovelItemViewModel>
{
    public static NovelItemViewModel CreateInstance(Novel entry) => new(entry);

    public int TextLength => Entry.TextLength;

    public Task<NovelContent> ContentAsync { get; } = App.AppViewModel.MakoClient.GetNovelContentAsync(novel.Id);
}
