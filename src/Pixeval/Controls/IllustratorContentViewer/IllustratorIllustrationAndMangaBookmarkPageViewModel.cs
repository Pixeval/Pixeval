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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Controls.IllustratorContentViewer;

public class IllustratorIllustrationAndMangaBookmarkPageViewModel : ObservableObject, IDisposable
{
    public static readonly CountedTag EmptyCountedTag = new(new Tag(IllustratorIllustrationAndMangaBookmarkPageResources.EmptyCountedTagName, ""), 0);

    private readonly CancellationTokenSource _bookmarksIdLoadingCancellationTokenSource = new();

    private readonly ConcurrentDictionary<string, HashSet<long>> _bookmarkTagIllustrationIdDictionary = new();

    public ObservableCollection<CountedTag> UserBookmarkTags { get; } = [];

    public void Dispose()
    {
        _bookmarksIdLoadingCancellationTokenSource.Cancel();
    }

    public event EventHandler<string>? TagBookmarksIncrementallyLoaded;

    public async Task LoadUserBookmarkTagsAsync(long uid)
    {
        var result = await App.AppViewModel.MakoClient.GetUserSpecifiedBookmarkTagsAsync(uid);
        UserBookmarkTags.Add(EmptyCountedTag);
        UserBookmarkTags.AddRange(result.Where(t => t.Value is PrivacyPolicy.Public).Select(t => t.Key));
    }

    // Fuck Pixiv: The results from web API and the results from app API have different formats and json schemas,
    // so the only useful thing we can get from web API are the IDs of those illustrations belonging to the specific tag,
    // but the API is paged, which means we can get at most 100 IDs per request, so this is a gradual process, to prevent
    // from waiting for too long before all IDs are fetched, we choose an incremental way, i.e., instead of setting the filter
    // after all IDs are fetched, we update the filter whenever new IDs are available.
    public async Task LoadBookmarksForTagAsync(long uid, string tag)
    {
        if (_bookmarkTagIllustrationIdDictionary.TryGetValue(tag, out var set) && set.Count > 0)
        {
            return;
        }
        // fork a token from the source
        var token = _bookmarksIdLoadingCancellationTokenSource.Token;
        var engine = App.AppViewModel.MakoClient.UserTaggedBookmarksId(uid, tag);
        var counter = 0;
        var hashSet = _bookmarkTagIllustrationIdDictionary.GetOrAdd(tag, []);
        await foreach (var id in engine)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            // 100 IDs per page
            if (counter == 100)
            {
                counter = 0;
                // tells the UI that new IDs are available
                TagBookmarksIncrementallyLoaded?.Invoke(this, tag);
            }

            _ = hashSet.Add(id);
            counter++;
        }

        if (counter != 0)
        {
            TagBookmarksIncrementallyLoaded?.Invoke(this, tag);
        }
    }

    public IReadOnlySet<long> GetBookmarkIdsForTag(string tag)
    {
        return _bookmarkTagIllustrationIdDictionary[tag];
    }

    public static string GetCountedTagDisplayText(CountedTag tag)
    {
        return ReferenceEquals(tag, EmptyCountedTag) ? tag.Tag.Name : $"#{(tag.Tag.TranslatedName is { Length: > 0 } str ? str : tag.Tag.Name)} ({tag.Count})";
    }
}
