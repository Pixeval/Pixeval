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
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability;

public partial class BookmarksPageViewModel(long userId) : ObservableObject, IDisposable
{
    public long UserId { get; } = userId;

    public bool IsMe => App.AppViewModel.PixivUid == UserId;

    private readonly CancellationTokenSource _bookmarksIdLoadingCancellationTokenSource = new();

    private readonly ConcurrentDictionary<string, HashSet<long>> _bookmarkTagIllustrationIdDictionary = new();

    public IList<BookmarkTag>? IllustrationPrivateBookmarkTags { get; private set; }

    public IList<BookmarkTag>? IllustrationPublicBookmarkTags { get; private set; }

    public IList<BookmarkTag>? NovelPrivateBookmarkTags { get; private set; }

    public IList<BookmarkTag>? NovelPublicBookmarkTags { get; private set; }

    public async Task<IList<BookmarkTag>> SetBookmarkTagsAsync(PrivacyPolicy policy, SimpleWorkType type)
    {
        if (IsMe)
            return await MakoHelper.GetBookmarkTagsAsync(policy, type);

        var lazy = new Lazy<Task<List<BookmarkTag>>>(() => App.AppViewModel.MakoClient.GetBookmarkTagAsync(UserId, type, policy));
        return (policy, type) switch
        {
            (PrivacyPolicy.Private, SimpleWorkType.IllustAndManga) => IllustrationPrivateBookmarkTags ??= await lazy.Value,
            (PrivacyPolicy.Public, SimpleWorkType.IllustAndManga) => IllustrationPublicBookmarkTags ??= await lazy.Value,
            (PrivacyPolicy.Private, SimpleWorkType.Novel) => NovelPrivateBookmarkTags ??= await lazy.Value,
            (PrivacyPolicy.Public, SimpleWorkType.Novel) => NovelPublicBookmarkTags ??= await lazy.Value,
            _ => ThrowHelper.ArgumentOutOfRange<(PrivacyPolicy, SimpleWorkType), IList<BookmarkTag>>((policy, type))
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _bookmarksIdLoadingCancellationTokenSource.TryCancelDispose();
    }

    public event EventHandler<string>? TagBookmarksIncrementallyLoaded;

    /// <summary>
    /// Fuck Pixiv: The results from web API and the results from app API have different formats and json schemas,
    /// so the only useful thing we can get from web API are the IDs of those illustrations belonging to the specific tag,
    /// but the API is paged, which means we can get at most 100 IDs per request, so this is a gradual process, to prevent
    /// from waiting for too long before all IDs are fetched, we choose an incremental way, i.e., instead of settings the filter
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
