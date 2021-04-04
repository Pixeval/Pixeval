#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Pixeval.Core;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
using PropertyChanged;

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class IllustrationPageViewModel
    {
        
        public enum BrowseKind
        {
            [EnumLocalizedName("UserBrowserIllustSelector")]
            Upload, 
            
            [EnumLocalizedName("UserBrowserGallerySelector")]
            Bookmark
        }

        // Filter a collection of illustrations using provided tags
        // it tests whether an illustration has all the tags in Accepts
        // and does not contains any of tags in Rejects
        private class SelectedTagCollectionCondition : ICollectionCondition<Illustration>
        {
            private ISet<string> Accepts { get; }

            private ISet<string> Rejects { get; }

            public SelectedTagCollectionCondition(ISet<string> accepts, ISet<string> rejects)
            {
                Accepts = accepts;
                Rejects = rejects;
            }

            public bool CanAdd(Illustration item)
            {
                return Accepts.All(x => item.Tags.Any(t => t.Name == x || t.TranslatedName == x)) && Rejects.All(x => item.Tags.All(t => t.Name != x && t.TranslatedName != x));
            }
        }

        public IllustrationPageViewModel()
        {
            Browsing = new Observable<BrowseKind>(BrowseKind.Bookmark, (sender, args) =>
            {
                CurrentlyViewing = args.NewValue switch
                {
                    BrowseKind.Upload   => Uploads,
                    BrowseKind.Bookmark => Bookmarks,
                    _                   => throw new ArgumentOutOfRangeException()
                };
                SelectedTags.Clear();
            });
        }

        private readonly ObservableCollection<CountedTag> allPossibleTagsForUpload = new ObservableCollection<CountedTag>();

        public IReadOnlyList<CountedTag> AllPossibleTagsForUpload => allPossibleTagsForUpload;

        private readonly ObservableCollection<CountedTag> allPossibleTagsForBookmark = new ObservableCollection<CountedTag>();

        public IReadOnlyList<CountedTag> AllPossibleTagsForBookmark => allPossibleTagsForBookmark;

        public ConditionalObservableCollection<Illustration> Uploads { get; set; }
        
        // Bookmarks use custom tags
        public ObservableCollection<Illustration> Bookmarks { get; set; }
        
        public ObservableCollection<Illustration> CurrentlyViewing { get; set; }

        // it's mostly Settings.Global.ExcludeTags, since the illustrator page does not provide such functionality that allows
        // user to add exclude tags
        public IReadOnlyList<string> RejectTags { get; set; }
        
        public ObservableCollection<CountedTag> SelectedTags { get; set; }

        public Observable<BrowseKind> Browsing { get; }
        
        public BitmapImage IllustratorAvatar { get; set; }
        
        public User Illustrator { get; set; }

        public IEnumerable<CountedTag> GetSuggestionTags(string input)
        {
            return (Browsing.Value switch
            {
                BrowseKind.Upload   => AllPossibleTagsForUpload,
                BrowseKind.Bookmark => AllPossibleTagsForBookmark,
                _                   => throw new ArgumentOutOfRangeException()
            }).Where(t => t.Name.Contains(input) || t.TranslatedName?.Contains(input) is true);
        }
        
        public async Task SetIllustratorAvatar()
        {
            IllustratorAvatar = await PixivIO.FromUrl(Illustrator.Avatar);
        }
        
        public async Task FillBookmarkTags()
        {
            var tags = await HttpClientFactory.WebApiService.GetBookmarkTagsForUser(Illustrator.Id, new CultureInfo(Settings.Global.Culture).TwoLetterISOLanguageName);
            var hashset = new HashSet<CountedTag>(new CountedTagEqualityComparer());
            hashset.AddRange(tags.ResponseBody.PublicTags.Select(tag => new CountedTag(tag.Tag, null, tag.Count)));
            hashset.AddRange(tags.ResponseBody.PrivateTags.Select(tag => new CountedTag(tag.Tag, null, tag.Count)));
            allPossibleTagsForBookmark.AddRange(hashset);
        }

        public async Task FillUploadTags()
        {
            var tags = await HttpClientFactory.WebApiService.GetUploadTagsForUser(Illustrator.Id, new CultureInfo(Settings.Global.Culture).TwoLetterISOLanguageName);
            allPossibleTagsForUpload.AddRange(tags.ResponseBody.Select(tag => new CountedTag(tag.Tag, tag.TagTranslation, tag.Count)));
        }
        
        public async Task AddTagToFilter(CountedTag tag)
        {
            using var semaphore = new SemaphoreSlim(1, 1);
            await semaphore.WaitAsync();
            switch (Browsing.Value)
            {
                case BrowseKind.Upload:
                    if (SelectedTags.Contains(tag))
                    {
                        SelectedTags.Add(tag);
                        await OnFilterTagChanged();
                    }
                    break;
                case BrowseKind.Bookmark:
                    if (SelectedTags[0].Equals(tag))
                    {
                        SelectedTags.Clear();
                        SelectedTags.Add(tag);
                        await OnFilterTagChanged();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public async Task RemoveTagFromFilter(CountedTag tag)
        {
            using var semaphore = new SemaphoreSlim(1, 1);
            await semaphore.WaitAsync();
            if (SelectedTags.Remove(tag))
            {
                await OnFilterTagChanged();
            }
        }
        
        // invokes when tag list is updated
        private async Task OnFilterTagChanged()
        {
            switch (Browsing.Value)
            {
                case BrowseKind.Upload:
                    Uploads.Predicate = BuildCondition();
                    Uploads.Refresh();
                    break;
                case BrowseKind.Bookmark:
                    Bookmarks = new ObservableCollection<Illustration>(await PixivClient.GetBookmarksWithTag(Illustrator.Id, SelectedTags[0].Name, new CultureInfo(Settings.Global.Culture).TwoLetterISOLanguageName));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private ICollectionCondition<Illustration> BuildCondition()
        {
            return new SelectedTagCollectionCondition(SelectedTags.Select(tag => tag.Name).Where(tag => !tag.IsNullOrEmpty()).ToHashSet(), RejectTags.Where(tag => !tag.IsNullOrEmpty()).ToHashSet());
        }

        private class CountedTagEqualityComparer : IEqualityComparer<CountedTag>
        {
            public bool Equals(CountedTag x, CountedTag y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.GetType() != y.GetType())
                {
                    return false;
                }
                return x.Name == y.Name;
            }

            public int GetHashCode(CountedTag obj)
            {
                return obj.Name != null ? obj.Name.GetHashCode() : 0;
            }
        }
    }
}