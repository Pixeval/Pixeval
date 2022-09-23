﻿#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/FeedEngine.cs
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
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

internal class FeedEngine : AbstractPixivFetchEngine<Feed>
{
    public FeedEngine(MakoClient makoClient, EngineHandle? engineHandle) : base(makoClient, engineHandle)
    {
    }

    public override IAsyncEnumerator<Feed> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new UserFeedsAsyncEnumerator(this, MakoApiKind.WebApi)!;
    }

    private class UserFeedsAsyncEnumerator : AbstractPixivAsyncEnumerator<Feed, string, FeedEngine>
    {
        private FeedRequestContext? _feedRequestContext;
        private string? _tt;


        public UserFeedsAsyncEnumerator(FeedEngine pixivFetchEngine, MakoApiKind apiKind) : base(pixivFetchEngine, apiKind)
        {
        }

        public override async ValueTask<bool> MoveNextAsync()
        {
            if (_feedRequestContext is null)
            {
                switch (await GetResponseAsync(BuildRequestUrl()).ConfigureAwait(false))
                {
                    case Result<string>.Success(var response):
                        if (TryParsePreloadJsonFromHtml(response, out var result))
                        {
                            await UpdateAsync(result).ConfigureAwait(false);
                            _tt = Regex.Match(response, "tt: \"(?<tt>.*)\"").Groups["tt"].Value;
                            _feedRequestContext = ExtractRequestContextFromHtml(response);
                        }
                        else
                        {
                            PixivFetchEngine.EngineHandle.Complete();
                            return false;
                        }

                        break;
                    case Result<string>.Failure(var exception):
                        if (exception is { } e)
                        {
                            PixivFetchEngine.EngineHandle.Complete();
                            throw e;
                        }

                        PixivFetchEngine.EngineHandle.Complete();
                        return false;
                }
            }

            if (CurrentEntityEnumerator!.MoveNext())
            {
                return true;
            }

            if (_feedRequestContext!.IsLastPage)
            {
                PixivFetchEngine.EngineHandle.Complete();
                return false;
            }

            if (await GetResponseAsync(BuildRequestUrl()).ConfigureAwait(false) is Result<string>.Success(var json)) // Else request a new page
            {
                if (IsCancellationRequested)
                {
                    PixivFetchEngine.EngineHandle.Complete();
                    return false;
                }

                await UpdateAsync(ParseFeedJson(JsonDocument.Parse(json).RootElement.GetProperty("stacc"))).ConfigureAwait(false);
                _feedRequestContext = ExtractRequestContextFromJsonElement(JsonDocument.Parse(json).RootElement.GetProperty("stacc"));
                return true;
            }

            PixivFetchEngine.EngineHandle.Complete();
            return false;
        }

        private static FeedRequestContext? ExtractRequestContextFromHtml(string html)
        {
            if (TryExtractPreloadJson(html, out var json))
            {
                try
                {
                    return ExtractRequestContextFromJsonElement(JsonDocument.Parse(json).RootElement);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static FeedRequestContext? ExtractRequestContextFromJsonElement(JsonElement stacc)
        {
            var mode = stacc.GetProperty("param").GetPropertyString("mode");
            var unifyToken = stacc.GetProperty("param").GetPropertyString("unify_token");
            var sid = stacc.GetPropertyString("next_max_sid");
            var isLastPage = stacc.GetPropertyLong("is_last_page") == 1;
            return unifyToken is null || sid is null || mode is null ? null : new FeedRequestContext(unifyToken, sid, mode, isLastPage);
        }

        private static bool TryParsePreloadJsonFromHtml(string html, out IEnumerable<Task<Feed?>> result)
        {
            if (TryExtractPreloadJson(html, out var json))
            {
                result = ParseFeedJson(JsonDocument.Parse(json).RootElement);
                return true;
            }

            result = Array.Empty<Task<Feed?>>();
            return false;
        }

        private static bool TryExtractPreloadJson(string html, out string json)
        {
            var match = Regex.Match(html, "pixiv\\.stacc\\.env\\.preload\\.stacc \\= (?<json>.*);");
            if (match.Success)
            {
                json = match.Groups["json"].Value;
                return true;
            }

            json = string.Empty;
            return false;
        }

        private static IEnumerable<Task<Feed?>> ParseFeedJson(JsonElement stacc)
        {
            var users = stacc.GetPropertyOrNull("user").EnumerateObjectOrEmpty();
            var illusts = stacc.GetPropertyOrNull("illust").EnumerateObjectOrEmpty();
            var novels = stacc.GetPropertyOrNull("novel").EnumerateObjectOrEmpty();
            var statuses = stacc.GetPropertyOrNull("status").EnumerateObjectOrEmpty();
            var timelines = stacc.GetPropertyOrNull("timeline").EnumerateObjectOrEmpty();

            return timelines.SelectNotNull(timeline => Task.Run(() => ParseFeed(timeline)));

            Feed? ParseFeed(JsonProperty timeline)
            {
                var id = timeline.Name;
                var status = statuses.FirstOrNull(st => st.Name == id);
                if (!status.HasValue)
                {
                    return null;
                }

                FeedType? feedType = status.Value.GetPropertyString("type") switch
                {
                    "add_bookmark" => FeedType.AddBookmark,
                    "add_illust" => FeedType.AddIllust,
                    "add_novel_bookmark" => FeedType.AddNovelBookmark,
                    "add_favorite" => FeedType.AddFavorite,
                    _ => null
                };
                var feedTargetId = feedType switch
                {
                    FeedType.AddBookmark or FeedType.AddIllust => status.Value.GetProperty("ref_illust").GetPropertyString("id"),
                    FeedType.AddFavorite => status.Value.GetProperty("ref_user").GetPropertyLong("id").ToString(), // long & string in two objects with almost the same properties? fuck pixiv
                    FeedType.AddNovelBookmark => status.Value.GetProperty("ref_novel").GetPropertyString("id"),
                    _ => null
                };
                if (feedTargetId is null)
                {
                    return null; // a feed with null target ID is considered useless because we cannot track its target
                }

                var feedTargetThumbnail = feedType switch
                {
                    FeedType.AddBookmark or FeedType.AddIllust => illusts.FirstOrNull(i => i.Name == feedTargetId)
                        ?.GetPropertyOrNull("url")
                        ?.GetPropertyOrNull("m")
                        ?.GetString(),
                    FeedType.AddFavorite => users.FirstOrNull(u => u.Name == feedTargetId)
                        ?.GetPropertyOrNull("profile_image")
                        .EnumerateObjectOrEmpty()
                        .FirstOrNull()
                        ?.GetPropertyOrNull("url")
                        ?.GetPropertyOrNull("m")
                        ?.GetString(),
                    FeedType.AddNovelBookmark => novels.FirstOrNull(n => n.Name == feedTargetId)
                        ?.GetPropertyOrNull("url")
                        ?.GetPropertyOrNull("m")
                        ?.GetString(),
                    _ => null
                };
                var postDate = DateTime.ParseExact(status.Value.GetPropertyString("post_date")!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
                var postUserId = status.Value.GetProperty("post_user").GetPropertyLong("id").ToString();
                var postUser = users.FirstOrNull(u => u.Name == postUserId);
                if (!postUser.HasValue)
                {
                    return null;
                }

                var postUserName = postUser.Value.GetPropertyString("name");
                var postUserThumbnail = postUser.Value
                    .GetPropertyOrNull("profile_image")
                    .EnumerateObjectOrEmpty()
                    .FirstOrNull()
                    ?.GetPropertyOrNull("url")
                    ?.GetPropertyOrNull("m")
                    ?.GetString();
                var feedObject = new Feed
                {
                    FeedId = feedTargetId,
                    FeedThumbnail = feedTargetThumbnail,
                    Type = feedType,
                    PostDate = postDate,
                    PostUserId = postUserId,
                    PostUserName = postUserName,
                    PostUserThumbnail = postUserThumbnail
                };

                switch (feedType)
                {
                    case FeedType.AddBookmark or FeedType.AddIllust:
                        {
                            var illustration = illusts.FirstOrNull(i => i.Name == feedTargetId);
                            feedObject.ArtistName = users.FirstOrNull(u => u.Name == (illustration?.GetPropertyOrNull("post_user")?.GetPropertyOrNull("id")?.GetString() ?? string.Empty))?.GetPropertyOrNull("name")?.GetString();
                            feedObject.FeedName = illustration?.GetPropertyOrNull("title")?.GetString();
                            break;
                        }
                    case FeedType.AddFavorite:
                        feedObject.FeedName = users.FirstOrNull(u => u.Name == feedTargetId)?.GetPropertyOrNull("name")?.GetString();
                        feedObject.IsTargetRefersToUser = true;
                        break;
                }

                return feedObject;
            }
        }

        private async Task UpdateAsync(IEnumerable<Task<Feed?>> result)
        {
            CurrentEntityEnumerator = (await Task.WhenAll(result).ConfigureAwait(false)).WhereNotNull().GetEnumerator();
            PixivFetchEngine.RequestedPages++;
        }

        protected override bool ValidateResponse(string rawEntity)
        {
            throw new NotSupportedException();
        }

        private string BuildRequestUrl()
        {
            return _feedRequestContext is null
                ? "/stacc?mode=unify"
                : $"/stacc/my/home/all/activity/{_feedRequestContext.Sid}/.json"
                  + $"?mode={_feedRequestContext.Mode}"
                  + $"&unify_token={_feedRequestContext.UnifyToken}"
                  + $"&tt={_tt}";
        }

        private async Task<Result<string>> GetResponseAsync(string url)
        {
            try
            {
                return await MakoClient.ResolveKeyed<HttpClient>(MakoApiKind.WebApi)
                    .GetStringResultAsync(url, async responseMessage => new MakoNetworkException(url, MakoClient.Configuration.Bypass, await responseMessage.Content.ReadAsStringAsync(), (int)responseMessage.StatusCode))
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                return Result<string>.OfFailure(new MakoNetworkException(url, MakoClient.Configuration.Bypass, e.Message, (int?)e.StatusCode ?? -1));
            }
        }
    }

    /// <summary>
    ///     Required parameters established from multiple tests, I don't know what do they mean
    /// </summary>
    private record FeedRequestContext
    {
        public FeedRequestContext(string unifyToken, string sid, string mode, bool isLastPage)
    {
        UnifyToken = unifyToken;
        Sid = sid;
        Mode = mode;
        IsLastPage = isLastPage;
    }

    public string UnifyToken { get; }

    public string Sid { get; }

    public string Mode { get; }

    public bool IsLastPage { get; }
}
}