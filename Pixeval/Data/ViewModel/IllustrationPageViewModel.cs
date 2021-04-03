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
using System.Linq;
using System.Runtime.CompilerServices;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;
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
        // and does not contains any of the Rejects
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

        private readonly ObservableCollection<string> allPossibleTagsForUpload = new ObservableCollection<string>();

        public IReadOnlyList<string> AllPossibleTagsForUpload => allPossibleTagsForUpload;

        private readonly ObservableCollection<string> allPossibleTagsForBookmark = new ObservableCollection<string>();

        public IReadOnlyList<string> AllPossibleTagsForBookmark => allPossibleTagsForBookmark;

        public ConditionalObservableCollection<Illustration> Uploads { get; set; }
        
        public ConditionalObservableCollection<Illustration> Bookmarks { get; set; }

        // it's mostly Settings.Global.ExcludeTags, since the illustrator page does not provide such functionality that allows
        // user to add exclude tags
        public IReadOnlyList<string> RejectTags { get; set; }
        
        public ObservableCollection<string> SelectedTags { get; set; }
        
        public BrowseKind Browsing { get; set; }
        
        public User Illustrator { get; set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddTagToFilter(string tag)
        {
            if (SelectedTags.Contains(tag))
            {
                SelectedTags.Add(tag);
                OnFilterTagChanged();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveTagFromFilter(string tag)
        {
            if (SelectedTags.Remove(tag))
            {
                OnFilterTagChanged();
            }
        }
        
        // invokes when tag list is updated, it refresh the condition by using 
        // updated tags
        private void OnFilterTagChanged()
        {
            var list = Browsing switch
            {
                BrowseKind.Upload   => Uploads,
                BrowseKind.Bookmark => Bookmarks,
                _                   => throw new ArgumentOutOfRangeException()
            };
            list.Predicate = BuildCondition();
            list.Refresh();
        }
        
        private ICollectionCondition<Illustration> BuildCondition()
        {
            return new SelectedTagCollectionCondition(SelectedTags.ToHashSet(), RejectTags.ToHashSet());
        }
    }
}