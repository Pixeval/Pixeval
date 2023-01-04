#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IIllustrationViewViewModel.cs
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

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.UserControls.IllustrationView;

public abstract partial class IllustrationViewViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private bool _hasNoItems;

    [ObservableProperty]
    private string? _selectionLabel;

    [ObservableProperty]
    private bool _isAnyIllustrationSelected;

    public abstract IIllustrationViewDataProvider DataProvider { get; set; }

    public async Task ResetEngineAndFillAsync(IFetchEngine<Illustration?>? newEngine, int? itemLimit = null)
    {
        HasNoItems = await DataProvider.ResetAndFillAsync(newEngine, itemLimit) == 0;
    }

    public abstract void Dispose();
}

public abstract class SortableIllustrationViewViewModel : IllustrationViewViewModel
{
    public abstract void SetSortDescription(SortDescription description);

    public abstract void ClearSortDescription();
}