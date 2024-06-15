#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/NovelItemViewModel.cs
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
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public partial class NovelItemViewModel(Novel novel) : WorkEntryViewModel<Novel>(novel), IViewModelFactory<Novel, NovelItemViewModel>
{
    public static NovelItemViewModel CreateInstance(Novel entry, int _) => new(entry);

    public int TextLength => Entry.TextLength;

    public NovelContent? Content { get; private set; }

    public async Task<NovelContent> GetNovelContentAsync()
    {
        return Content ??= await App.AppViewModel.MakoClient.GetNovelContentAsync(Id);
    }
}
