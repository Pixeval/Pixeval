#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorIllustrationAndMangaBookmarkPageViewModel.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Options;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public class BookmarkPageViewModel(long userId) : ObservableObject, IDisposable
{
    public static readonly BookmarkTag EmptyCountedTag = new() { Name = BookmarksPageResources.EmptyCountedTagName, Count = 0 };

    public long UserId { get; } = userId;

    public bool IsMe => App.AppViewModel.PixivUid == UserId;

    private readonly CancellationTokenSource _bookmarksIdLoadingCancellationTokenSource = new();

    private readonly ConcurrentDictionary<string, HashSet<long>> _bookmarkTagIllustrationIdDictionary = new();

    public BookmarkTag[]? IllustrationPrivateBookmarkTags { get; set; }

    public BookmarkTag[]? IllustrationPublicBookmarkTags { get; set; }

    public BookmarkTag[]? NovelPrivateBookmarkTags { get; set; }

    public BookmarkTag[]? NovelPublicBookmarkTags { get; set; }

    public async Task<BookmarkTag[]> SetBookmarkTagsAsync(PrivacyPolicy policy, SimpleWorkType type)
    {
        return (policy, type) switch
        {
            (PrivacyPolicy.Private, SimpleWorkType.IllustAndManga) => IllustrationPrivateBookmarkTags ??= [EmptyCountedTag, .. await App.AppViewModel.MakoClient.IllustrationBookmarkTag(UserId, policy).ToArrayAsync()],
            (PrivacyPolicy.Public, SimpleWorkType.IllustAndManga) => IllustrationPublicBookmarkTags ??= [EmptyCountedTag, .. await App.AppViewModel.MakoClient.IllustrationBookmarkTag(UserId, policy).ToArrayAsync()],
            (PrivacyPolicy.Private, SimpleWorkType.Novel) => NovelPrivateBookmarkTags ??= [EmptyCountedTag, .. await App.AppViewModel.MakoClient.NovelBookmarkTag(UserId, policy).ToArrayAsync()],
            (PrivacyPolicy.Public, SimpleWorkType.Novel) => NovelPublicBookmarkTags ??= [EmptyCountedTag, .. await App.AppViewModel.MakoClient.NovelBookmarkTag(UserId, policy).ToArrayAsync()],
            _ => ThrowHelper.ArgumentOutOfRange<(PrivacyPolicy, SimpleWorkType), BookmarkTag[]>((policy, type))
        };
    }

    public void Dispose()
    {
        _bookmarksIdLoadingCancellationTokenSource.Cancel();
    }

    public event EventHandler<string>? TagBookmarksIncrementallyLoaded;

    /// <summary>
    /// Fuck Pixiv: The results from web API and the results from app API have different formats and json schemas,
    /// so the only useful thing we can get from web API are the IDs of those illustrations belonging to the specific tag,
    /// but the API is paged, which means we can get at most 100 IDs per request, so this is a gradual process, to prevent
    /// from waiting for too long before all IDs are fetched, we choose an incremental way, i.e., instead of setting the filter
    /// after all IDs are fetched, we update the filter whenever new IDs are available.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="engine"></param>
    /// <returns></returns>
    public async Task LoadBookmarksForTagAsync(string tag, IFetchEngine<IWorkEntry> engine)
    {
        if (_bookmarkTagIllustrationIdDictionary.TryGetValue(tag, out var set) && set.Count > 0)
            return;
        // fork a token from the source
        var token = _bookmarksIdLoadingCancellationTokenSource.Token;
        var counter = 0;
        var hashSet = _bookmarkTagIllustrationIdDictionary.GetOrAdd(tag, []);
        await foreach (var entry in engine)
        {
            if (token.IsCancellationRequested)
                break;

            // 100 IDs per page
            if (counter is 100)
            {
                counter = 0;
                // tells the UI that new IDs are available
                TagBookmarksIncrementallyLoaded?.Invoke(this, tag);
            }

            _ = hashSet.Add(entry.Id);
            ++counter;
        }

        if (counter is not 0)
            TagBookmarksIncrementallyLoaded?.Invoke(this, tag);
    }

    public bool ContainsTag(string tag, long id) => _bookmarkTagIllustrationIdDictionary[tag].Contains(id);
}
