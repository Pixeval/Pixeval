﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

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
using System.Linq;
using Newtonsoft.Json;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;

namespace Pixeval.Data.Web.Response
{
    public class RecommendResponse
    {
        [JsonProperty("illusts")]
        public List<Illust> Illusts { get; set; }

        [JsonProperty("contest_exists")]
        public bool ContestExists { get; set; }

        [JsonProperty("next_url")]
        public string NextUrl { get; set; }

        public class Illust : IParser<Illustration>
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("image_urls")]
            public ImageUrls ImageUrls { get; set; }

            [JsonProperty("caption")]
            public string Caption { get; set; }

            [JsonProperty("restrict")]
            public long Restrict { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("tags")]
            public List<Tag> Tags { get; set; }

            [JsonProperty("tools")]
            public List<string> Tools { get; set; }

            [JsonProperty("create_date")]
            public DateTimeOffset CreateDate { get; set; }

            [JsonProperty("page_count")]
            public long PageCount { get; set; }

            [JsonProperty("width")]
            public long Width { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }

            [JsonProperty("sanity_level")]
            public long SanityLevel { get; set; }

            [JsonProperty("x_restrict")]
            public long XRestrict { get; set; }

            [JsonProperty("meta_single_page")]
            public MetaSinglePage MetaSinglePage { get; set; }

            [JsonProperty("meta_pages")]
            public List<MetaPage> MetaPages { get; set; }

            [JsonProperty("total_view")]
            public long TotalView { get; set; }

            [JsonProperty("total_bookmarks")]
            public long TotalBookmarks { get; set; }

            [JsonProperty("is_bookmarked")]
            public bool IsBookmarked { get; set; }

            [JsonProperty("visible")]
            public bool Visible { get; set; }

            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }

            public Illustration Parse()
            {
                return new Illustration
                {
                    Bookmark = (int) TotalBookmarks,
                    Id = Id.ToString(),
                    IsLiked = IsBookmarked,
                    IsManga = PageCount != 1,
                    IsUgoira = Type == "ugoira",
                    Origin = MetaSinglePage.OriginalImageUrl.IsNullOrEmpty() ? MetaPages[0].ImageUrls.Original : MetaSinglePage.OriginalImageUrl,
                    Large = ImageUrls.Large,
                    Tags = Tags.Select(t => new ViewModel.Tag { Name = t.Name, TranslatedName = t.TranslatedName }),
                    Thumbnail = ImageUrls.Medium,
                    Title = Title,
                    UserId = User.Id.ToString(),
                    UserName = User.Name,
                    Resolution = $"{Width}x{Height}",
                    ViewCount = (int) TotalView,
                    PublishDate = CreateDate
                }.Apply(i =>
                {
                    if (i.IsManga)
                    {
                        i.MangaMetadata = MetaPages.Select(p =>
                        {
                            var page = (Illustration) i.Clone();
                            page.Thumbnail = p.ImageUrls.Medium;
                            page.Origin = p.ImageUrls.Original;
                            page.Large = p.ImageUrls.Large;
                            return page;
                        }).ToArray();
                        foreach (var illustration in i.MangaMetadata)
                        {
                            illustration.MangaMetadata = i.MangaMetadata;
                        }
                    }
                });
            }
        }

        public class ImageUrls
        {
            [JsonProperty("square_medium")]
            public string SquareMedium { get; set; }

            [JsonProperty("medium")]
            public string Medium { get; set; }

            [JsonProperty("large")]
            public string Large { get; set; }

            [JsonProperty("original", NullValueHandling = NullValueHandling.Ignore)]
            public string Original { get; set; }
        }

        public class MetaPage
        {
            [JsonProperty("image_urls")]
            public ImageUrls ImageUrls { get; set; }
        }

        public class MetaSinglePage
        {
            [JsonProperty("original_image_url", NullValueHandling = NullValueHandling.Ignore)]
            public string OriginalImageUrl { get; set; }
        }

        public class Tag
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("translated_name")]
            public string TranslatedName { get; set; }
        }

        public class User
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls ProfileImageUrls { get; set; }

            [JsonProperty("is_followed")]
            public bool IsFollowed { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonProperty("medium")]
            public string Medium { get; set; }
        }
    }
}