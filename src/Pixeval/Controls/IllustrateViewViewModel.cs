#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrateViewViewModel.cs
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
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public abstract partial class IllustrateViewViewModel<T, TViewModel> : ObservableObject, IDisposable where T : IIllustrate where TViewModel : IllustrateViewModel<T>
{
    [ObservableProperty]
    private bool _hasNoItems;

    /// <summary>
    /// Avoid calls to <see cref="DataProvider{T, TViewModel}.ResetAndFillAsync"/>, calls to <see cref="ResetEngineAndFillAsync"/> instead.
    /// </summary>
    public abstract DataProvider<T, TViewModel> DataProvider { get; }

    public abstract void Dispose();

    public async Task ResetEngineAndFillAsync(IFetchEngine<T?>? newEngine, int itemLimit = -1)
    {
        HasNoItems = await DataProvider.ResetAndFillAsync(newEngine, itemLimit) is 0;
    }
}
