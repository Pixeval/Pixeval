// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Utilities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Pixeval.Controls;

public partial class WorkEntryViewModel<T>
{
    private readonly Debounce<BookmarkDebounceTag, bool> _bookmarkDebounce = new();

    public enum BookmarkDebounceGroupPhase
    {
        Bookmark, RemoveBookmark
    }

    private record struct BookmarkDebounceTag(long EntryId, BookmarkDebounceGroupPhase Phase);

    private class BookmarkDebounceTask(WorkEntryViewModel<T> vm, bool isPrivate, IEnumerable<string>? userTags) : IDebouncedTask<BookmarkDebounceTag, bool>
    {
        public BookmarkDebounceTag Id => new(vm.Id, BookmarkDebounceGroupPhase.Bookmark);
        public BookmarkDebounceTag? Dependency => null;

        public Task<bool> ExecuteAsync()
        {
            return vm.SetBookmarkAsync(vm.Id, true, isPrivate, userTags);
        }

        public bool IsFinalizer => false;

        public bool IsHead => true;
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

        public bool IsHead => false;
    }

    protected override void DisposeOverride()
    {
        _bookmarkDebounce.Dispose();
    }
}
