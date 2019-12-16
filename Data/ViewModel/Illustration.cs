// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using Pixeval.Data.Web.Response;
using PropertyChanged;

#pragma warning disable 8509

namespace Pixeval.Data.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class Illustration : ICloneable
    {
        public string Id { get; set; }

        public bool IsUgoira { get; set; }

        public string Origin { get; set; }

        public string Thumbnail { get; set; }

        public int Bookmark { get; set; }

        public bool IsLiked { get; set; }

        public bool IsManga { get; set; }

        public string Title { get; set; }

        public IllustType Type { get; set; }

        public string UserName { get; set; }

        public string UserId { get; set; }

        public string[] Tags { get; set; }

        public Illustration[] MangaMetadata { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public class IllustType
        {
            public static readonly IllustType Illust = new IllustType();

            public static readonly IllustType Ugoira = new IllustType();

            public static readonly IllustType Manga = new IllustType();

            public static IllustType Parse(IllustResponse.Response illustration)
            {
                return illustration switch
                {
                    { Type: "illustration", IsManga: true } => Manga,
                    { Type: "manga"}                        => Manga,
                    { Type: "ugoira" }                      => Ugoira,
                    { Type: "illustration", IsManga: false} => Illust
                };
            }
        }
    }

    public class IllustrationComparator : IComparer<Illustration>
    {
        public static readonly IllustrationComparator Instance = new IllustrationComparator();

        public int Compare(Illustration x, Illustration y)
        {
            if (x == null || y == null) return 0;

            return x.Bookmark < y.Bookmark ? 1 : x.Bookmark == y.Bookmark ? 0 : -1;
        }
    }
}