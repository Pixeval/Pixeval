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
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public abstract class EntryViewViewModel<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TViewModel>
    : ObservableObject, IDisposable
    where T : class, IIdEntry
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>
{
    /// <summary>
    /// Avoid calls to <see cref="IDataProvider{T,TViewModel}.ResetEngine"/>, calls to <see cref="ResetEngine"/> instead.
    /// </summary>
    public abstract IDataProvider<T, TViewModel> DataProvider { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DataProvider.Dispose();
    }

    public void ResetEngine(IFetchEngine<T>? newEngine, int itemsPerPage = 20, int itemLimit = -1) => DataProvider.ResetEngine(newEngine, itemsPerPage, itemLimit);

    /// <summary>
    /// 不用!<see cref="AdvancedObservableCollection{T}.HasMoreItems"/>，此处只是为了表示集合有没有元素
    /// </summary>
    public bool HasNoItem => DataProvider.View.Count is 0;
}
