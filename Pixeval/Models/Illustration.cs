// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.Collections.Generic;
using Pixeval.Objects;
using PropertyChanged;

#pragma warning disable 8509

namespace Pixeval.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Illustration : ICloneable
    {
        public string Id { get; set; }

        public bool IsUgoira { get; set; }

        public string Origin { get; set; }

        public string Large { get; set; }

        public string Thumbnail { get; set; }

        public int Bookmark { get; set; }

        public bool IsLiked { get; set; }

        public bool IsManga { get; set; }

        public string Title { get; set; }

        public string UserName { get; set; }

        public string UserId { get; set; }

        public IEnumerable<Tag> Tags { get; set; }

        public Illustration[] MangaMetadata { get; set; }

        public DateTimeOffset PublishDate { get; set; }

        public int ViewCount { get; set; }

        public string Resolution { get; set; }

        public int Comments { get; set; }

        public bool FromSpotlight { get; set; }

        public string SpotlightTitle { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public string GetDownloadUrl()
        {
            return Origin.IsNullOrEmpty() ? Large : Origin;
        }
    }

    public class Tag
    {
        public string Name { get; set; }

        public string TranslatedName { get; set; }
    }

    public class IllustrationPopularityComparator : IComparer<Illustration>
    {
        public static readonly IllustrationPopularityComparator Instance = new IllustrationPopularityComparator();

        public int Compare(Illustration x, Illustration y)
        {
            if (x == null || y == null) return 0;

            return x.Bookmark < y.Bookmark ? 1 : x.Bookmark == y.Bookmark ? 0 : -1;
        }
    }

    public class IllustrationPublishDateComparator : IComparer<Illustration>
    {
        public static readonly IllustrationPublishDateComparator Instance = new IllustrationPublishDateComparator();

        public int Compare(Illustration x, Illustration y)
        {
            if (x == null || y == null) return 0;
            return x.PublishDate < y.PublishDate ? 1 : x.PublishDate == y.PublishDate ? 0 : -1;
        }
    }
}