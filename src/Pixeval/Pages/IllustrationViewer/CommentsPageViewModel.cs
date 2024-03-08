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
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Options;

namespace Pixeval.Pages.IllustrationViewer;

public class CommentsPageViewModel(IAsyncEnumerable<Comment?> engine, CommentType type, long entryId)
{
    public long EntryId { get; } = entryId;

    public CommentType EntryType { get; } = type;

    public AdvancedObservableCollection<CommentBlockViewModel> View { get; } = new(
        new IncrementalLoadingCollection<CommentsIncrementalSource, CommentBlockViewModel>(
            new CommentsIncrementalSource(engine.Where(c => c is not null).Select(c => new CommentBlockViewModel(c!, type, entryId))), 30));

    public void AddComment(Comment comment)
    {
        View.Insert(0, new CommentBlockViewModel(comment, EntryType, EntryId));
    }

    public void DeleteComment(CommentBlockViewModel viewModel)
    {
        View.Remove(viewModel);
    }
}
