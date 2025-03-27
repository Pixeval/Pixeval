// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using Mako.Model;

namespace Pixeval.Controls;

public partial class NovelItemViewModel(Novel novel) : WorkEntryViewModel<Novel>(novel), IFactory<Novel, NovelItemViewModel>
{
    public static NovelItemViewModel CreateInstance(Novel entry) => new(entry);

    public int TextLength => Entry.TextLength;

    public NovelContent? Content { get; private set; }

    public async Task<NovelContent> GetNovelContentAsync()
    {
        return Content ??= await App.AppViewModel.MakoClient.GetNovelContentAsync(Id);
    }
}
