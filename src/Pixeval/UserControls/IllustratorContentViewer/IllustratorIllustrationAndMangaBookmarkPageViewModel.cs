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

namespace Pixeval.UserControls.IllustratorContentViewer;

public class IllustratorIllustrationAndMangaBookmarkPageViewModel : ObservableObject, IDisposable
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _bookmarkTagIllustrationIdDictionary;

    public static readonly CountedTag EmptyCountedTag = new(new Tag(IllustratorIllustrationAndMangaBookmarkPageResources.EmptyCountedTagName, string.Empty), 0);

    private readonly CancellationTokenSource _bookmarksIdLoadingCancellationTokenSource;

    public ObservableCollection<CountedTag> UserBookmarkTags { get; }

    private EventHandler<string>? _tagBookmarksIncrementallyLoaded;

    public event EventHandler<string> TagBookmarksIncrementallyLoaded
    {
        add => _tagBookmarksIncrementallyLoaded += value;
        remove => _tagBookmarksIncrementallyLoaded -= value;
    }

    public IllustratorIllustrationAndMangaBookmarkPageViewModel()
    {
        _bookmarkTagIllustrationIdDictionary = new ConcurrentDictionary<string, HashSet<string>>();
        _bookmarksIdLoadingCancellationTokenSource = new CancellationTokenSource();
        UserBookmarkTags = new ObservableCollection<CountedTag>();
    }

    public async Task LoadUserBookmarkTagsAsync(string uid)
    {
        var result = await App.AppViewModel.MakoClient.GetUserSpecifiedBookmarkTagsAsync(uid);
        UserBookmarkTags.Add(EmptyCountedTag);
        UserBookmarkTags.AddRange(result.Where(t => t.Value is PrivacyPolicy.Public).Select(t => t.Key));
    }

    public async Task LoadBookmarksForTagAsync(string uid, string tag)
    {
        if (_bookmarkTagIllustrationIdDictionary.TryGetValue(tag, out var set) && set.Count > 0)
        {
            return;
        }
        // fork a token from the source
        var token = _bookmarksIdLoadingCancellationTokenSource.Token;
        var engine = App.AppViewModel.MakoClient.UserTaggedBookmarksId(uid, tag);
        var counter = 0;
        var hashSet = _bookmarkTagIllustrationIdDictionary.GetOrAdd(tag, new HashSet<string>());
        await foreach (var id in engine)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            if (counter == 100)
            {
                counter = 0;
                _tagBookmarksIncrementallyLoaded?.Invoke(this, tag);
            }

            hashSet.Add(id);
            counter++;
        }

        if (counter != 0)
        {
            _tagBookmarksIncrementallyLoaded?.Invoke(this, tag);
        }
    }

    public IReadOnlySet<string> GetBookmarkIdsForTag(string tag)
    {
        return _bookmarkTagIllustrationIdDictionary[tag];
    }

    public static string GetCountedTagDisplayText(CountedTag tag)
    {
        return ReferenceEquals(tag, EmptyCountedTag) ? tag.Tag.Name : $"#{(tag.Tag.TranslatedName is { Length: > 0 } str ? str : tag.Tag.Name)} ({tag.Count})";
    }

    public void Dispose()
    {
        _bookmarksIdLoadingCancellationTokenSource.Cancel();
    }
}