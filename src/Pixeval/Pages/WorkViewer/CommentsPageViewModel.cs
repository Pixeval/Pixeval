#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentsPageViewModel.cs
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

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;

namespace Pixeval.Pages;

public class CommentsPageViewModel : ObservableObject
{
    public CommentsPageViewModel(IAsyncEnumerable<Comment?> engine, SimpleWorkType type, long entryId)
    {
        EntryId = entryId;
        EntryType = type;
        View = new(new IncrementalLoadingCollection<CommentsIncrementalSource, CommentBlockViewModel>(
            new CommentsIncrementalSource(engine.Where(c => c is not null)
                .Select(c => new CommentBlockViewModel(c!, type, entryId))), 30));
        View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public long EntryId { get; }

    public SimpleWorkType EntryType { get; }

    /// <summary>
    /// 不用!<see cref="AdvancedObservableCollection{T}.HasMoreItems"/>，此处只是为了表示集合有没有元素
    /// </summary>
    public bool HasNoItem => View.Count is 0;

    public AdvancedObservableCollection<CommentBlockViewModel> View { get; }

    public void AddComment(Comment comment)
    {
        View.Insert(0, new CommentBlockViewModel(comment, EntryType, EntryId));
    }

    public void DeleteComment(CommentBlockViewModel viewModel)
    {
        _ = View.Remove(viewModel);
    }
}
