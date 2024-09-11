#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/WorkEntryViewModel.Debounce.cs
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

using Pixeval.Utilities;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Pixeval.Controls;


public partial class WorkEntryViewModel<T>
{
    private readonly Debounce<BookmarkDebounceTag, bool> _bookmarkDebounce = new();

    public enum BookmarkDebounceGroupPhase
    {
        Bookmark, RemoveBookmark
    }

    private record struct BookmarkDebounceTag(long IllustId, BookmarkDebounceGroupPhase Phase);

    private class BookmarkDebounceTask(WorkEntryViewModel<T> vm, bool isPrivate, IEnumerable<string>? userTags) : IDebouncedTask<BookmarkDebounceTag, bool>
    {
        public BookmarkDebounceTag Id => new(vm.Id, BookmarkDebounceGroupPhase.Bookmark);
        public BookmarkDebounceTag? Dependency => null;

        public Task<bool> ExecuteAsync()
        {
            return vm.SetBookmarkAsync(vm.Id, true, isPrivate, userTags);
        }

        public bool IsFinalizer => false;
    }

    private class RemoveBookmarkDebounceTask(WorkEntryViewModel<T> vm, bool isPrivate, IEnumerable<string>? userTags) : IDebouncedTask<BookmarkDebounceTag, bool>
    {
        public BookmarkDebounceTag Id => new(vm.Id, BookmarkDebounceGroupPhase.RemoveBookmark);

        public BookmarkDebounceTag? Dependency => new(vm.Id, BookmarkDebounceGroupPhase.Bookmark);

        public Task<bool> ExecuteAsync()
        {
            return vm.SetBookmarkAsync(vm.Id, false, isPrivate, userTags);
        }

        public bool IsFinalizer => true;
    }
}
