#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IIllustratorViewViewModel.cs
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

using ABI.System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using System.Threading.Tasks;

namespace Pixeval.UserControls.IllustratorView;

public abstract partial class IllustratorViewViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private bool _hasNoItems;

    public abstract IIllustratorViewDataProvider DataProvider { get; }

    public abstract void Dispose();

    public async Task ResetEngineAndFillAsync(IFetchEngine<User?>? newEngine, int? itemLimit = null)
    {
        HasNoItems = await DataProvider.ResetAndFillAsync(newEngine, itemLimit) == 0;
    }
}