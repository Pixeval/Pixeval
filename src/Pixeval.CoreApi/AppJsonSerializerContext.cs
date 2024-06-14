#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2024 Pixeval.CoreApi/AppJsonSerializerContext.cs
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

using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using Pixeval.CoreApi.Preference;

namespace Pixeval.CoreApi;

[JsonSerializable(typeof(AutoCompletionResponse))]
[JsonSerializable(typeof(PixivBookmarkTagResponse))]
[JsonSerializable(typeof(PixivCommentResponse))]
[JsonSerializable(typeof(PixivIllustrationResponse))]
// [JsonSerializable(typeof(PixivNextUrlResponse<>))]
[JsonSerializable(typeof(PixivNovelResponse))]
[JsonSerializable(typeof(PixivRelatedUsersResponse))]
[JsonSerializable(typeof(PixivSingleIllustResponse))]
[JsonSerializable(typeof(PixivSingleNovelResponse))]
[JsonSerializable(typeof(PixivSingleUserResponse))]
[JsonSerializable(typeof(UserEntity))]
[JsonSerializable(typeof(Profile))]
[JsonSerializable(typeof(ProfilePublicity))]
[JsonSerializable(typeof(Workspace))]
[JsonSerializable(typeof(PixivSpotlightDetailResponse))]
[JsonSerializable(typeof(SpotlightBody))]
[JsonSerializable(typeof(Entry))]
[JsonSerializable(typeof(PixivisionCategory))]
[JsonSerializable(typeof(PixivisionSubcategory))]
[JsonSerializable(typeof(PixivisionTag))]
[JsonSerializable(typeof(Illust))]
[JsonSerializable(typeof(Url))]
[JsonSerializable(typeof(RelatedArticle))]
[JsonSerializable(typeof(PixivSpotlightResponse))]
[JsonSerializable(typeof(PixivUserResponse))]
[JsonSerializable(typeof(PostCommentResponse))]
[JsonSerializable(typeof(ReverseSearchResponse))]
[JsonSerializable(typeof(ReverseSearchResponseHeader))]
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Data))]
[JsonSerializable(typeof(ResultHeader))]
[JsonSerializable(typeof(TrendingTagResponse))]
[JsonSerializable(typeof(UgoiraMetadataResponse))]
[JsonSerializable(typeof(UgoiraMetadata))]
[JsonSerializable(typeof(Frame))]
[JsonSerializable(typeof(ZipUrls))]
[JsonSerializable(typeof(UserSpecifiedBookmarkTagResponse))]
[JsonSerializable(typeof(UserSpecifiedBookmarkTagBody))]
[JsonSerializable(typeof(UserSpecifiedBookmarkTag))]
[JsonSerializable(typeof(WebApiBookmarksWithTagResponse))]
[JsonSerializable(typeof(WebApiBookmarksWithTagBody))]
[JsonSerializable(typeof(Work))]

[JsonSerializable(typeof(BookmarkTag))]
[JsonSerializable(typeof(Feed))]
[JsonSerializable(typeof(FeedType))]
[JsonSerializable(typeof(IEntry))]
[JsonSerializable(typeof(IIdEntry))]
[JsonSerializable(typeof(IWorkEntry))]
[JsonSerializable(typeof(Illustration))]
[JsonSerializable(typeof(MetaSinglePage))]
[JsonSerializable(typeof(ImageUrls))]
[JsonSerializable(typeof(MangaImageUrls))]
[JsonSerializable(typeof(MetaPage))]
[JsonSerializable(typeof(XRestrict))]
[JsonSerializable(typeof(IllustrationType))]
[JsonSerializable(typeof(Novel))]
[JsonSerializable(typeof(Series))]
[JsonSerializable(typeof(NovelContent))]
[JsonSerializable(typeof(Rating))]
[JsonSerializable(typeof(SeriesNavigation))]
[JsonSerializable(typeof(NovelNavigation))]
[JsonSerializable(typeof(NovelImage))]
[JsonSerializable(typeof(NovelImageUrls))]
[JsonSerializable(typeof(NovelIllustInfo))]
[JsonSerializable(typeof(NovelIllust))]
[JsonSerializable(typeof(NovelTag))]
[JsonSerializable(typeof(NovelIllustUrls))]
[JsonSerializable(typeof(NovelUser))]
[JsonSerializable(typeof(NovelReplaceableGlossary))]
[JsonSerializable(typeof(ProfileImageUrls))]
[JsonSerializable(typeof(Spotlight))]
[JsonSerializable(typeof(SpotlightCategory))]
[JsonSerializable(typeof(SpotlightDetail))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(TrendingTag))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserInfo))]
[JsonSerializable(typeof(Comment))]
[JsonSerializable(typeof(Stamp))]
[JsonSerializable(typeof(CommentUser))]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(TokenUser))]
[JsonSerializable(typeof(TokenProfileImageUrls))]

[JsonSerializable(typeof(AddIllustBookmarkRequest))]
[JsonSerializable(typeof(AddNormalIllustCommentRequest))]
[JsonSerializable(typeof(AddNormalNovelCommentRequest))]
[JsonSerializable(typeof(AddNovelBookmarkRequest))]
[JsonSerializable(typeof(AddStampIllustCommentRequest))]
[JsonSerializable(typeof(AddStampNovelCommentRequest))]
[JsonSerializable(typeof(AutoCompletionRequest))]
[JsonSerializable(typeof(DeleteCommentRequest))]
[JsonSerializable(typeof(FollowUserRequest))]
[JsonSerializable(typeof(RefreshSessionRequest))]
[JsonSerializable(typeof(RemoveIllustBookmarkRequest))]
[JsonSerializable(typeof(RemoveFollowUserRequest))]
[JsonSerializable(typeof(RemoveNovelBookmarkRequest))]
[JsonSerializable(typeof(ReverseSearchRequest))]
[JsonSerializable(typeof(SingleUserRequest))]

[JsonSerializable(typeof(PrivacyPolicy))]
[JsonSerializable(typeof(RankOption))]
[JsonSerializable(typeof(SearchDuration))]
[JsonSerializable(typeof(SearchIllustrationTagMatchOption))]
[JsonSerializable(typeof(SearchNovelTagMatchOption))]
[JsonSerializable(typeof(TargetFilter))]
[JsonSerializable(typeof(WorkSortOption))]
[JsonSerializable(typeof(WorkType))]
[JsonSerializable(typeof(SimpleWorkType))]

[JsonSerializable(typeof(Session))]
public partial class AppJsonSerializerContext : JsonSerializerContext;

public class SnakeCaseLowerEnumConverter<T>() : JsonStringEnumConverter<T>(JsonNamingPolicy.SnakeCaseLower) where T : struct, Enum;
