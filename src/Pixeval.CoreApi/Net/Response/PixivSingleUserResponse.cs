// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Diagnostics;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

[DebuggerDisplay("{UserEntity}")]
[Factory]
public partial record PixivSingleUserResponse
{
    [JsonPropertyName("user")]
    public required UserEntity UserEntity { get; set; }

    [JsonPropertyName("profile")]
    public required Profile UserProfile { get; set; }

    [JsonPropertyName("profile_publicity")]
    public required ProfilePublicity UserProfilePublicity { get; set; }

    [JsonPropertyName("workspace")]
    public required Workspace UserWorkspace { get; set; }
}

[Factory]
public partial record UserEntity : UserInfo
{
    /// <summary>
    /// 比<see cref="UserInfo"/>多了个<see cref="Comment"/>
    /// </summary>
    [JsonPropertyName("comment")]
    public required string Comment { get; set; } = "";
}

[Factory]
public partial record Profile
{
    [JsonPropertyName("webpage")]
    public required string Webpage { get; set; } = "";

    [JsonPropertyName("gender")]
    public required string Gender { get; set; } = "";

    [JsonPropertyName("birth")]
    public required string Birth { get; set; } = "";

    [JsonPropertyName("birth_day")]
    public required string BirthDay { get; set; } = "";

    [JsonPropertyName("birth_year")]
    public required long BirthYear { get; set; }

    [JsonPropertyName("region")]
    public required string Region { get; set; } = "";

    [JsonPropertyName("address_id")]
    public required long AddressId { get; set; }

    [JsonPropertyName("country_code")]
    public required string CountryCode { get; set; } = "";

    [JsonPropertyName("job")]
    public required string Job { get; set; } = "";

    [JsonPropertyName("job_id")]
    public required long JobId { get; set; }

    [JsonPropertyName("total_follow_users")]
    public required int TotalFollowUsers { get; set; }

    /// <summary>
    /// 好P友
    /// </summary>
    [JsonPropertyName("total_mypixiv_users")]
    public required int TotalMyPixivUsers { get; set; }

    [JsonPropertyName("total_illusts")]
    public required int TotalIllusts { get; set; }

    [JsonPropertyName("total_manga")]
    public required int TotalManga { get; set; }

    [JsonPropertyName("total_novels")]
    public required int TotalNovels { get; set; }

    [JsonPropertyName("total_illust_bookmarks_public")]
    public required int TotalIllustBookmarksPublic { get; set; }

    [JsonPropertyName("total_illust_series")]
    public required int TotalIllustSeries { get; set; }

    [JsonPropertyName("total_novel_series")]
    public required int TotalNovelSeries { get; set; }

    [JsonPropertyName("background_image_url")]
    public required string? BackgroundImageUrl { get; set; }

    [JsonPropertyName("twitter_account")]
    public required string TwitterAccount { get; set; } = "";

    [JsonPropertyName("twitter_url")]
    public required string TwitterUrl { get; set; } = "";

    [JsonPropertyName("is_premium")]
    public required bool IsPremium { get; set; }

    [JsonPropertyName("is_using_custom_profile_image")]
    public required bool IsUsingCustomProfileImage { get; set; }
}

[Factory]
public partial record ProfilePublicity
{
    [JsonPropertyName("gender")]
    public required string Gender { get; set; } = "";

    [JsonPropertyName("region")]
    public required string Region { get; set; } = "";

    [JsonPropertyName("birth_day")]
    public required string BirthDay { get; set; } = "";

    [JsonPropertyName("birth_year")]
    public required string BirthYear { get; set; } = "";

    [JsonPropertyName("job")]
    public required string Job { get; set; } = "";

    [JsonPropertyName("pawoo")]
    public required bool Pawoo { get; set; }
}

[Factory]
public partial record Workspace
{
    [JsonPropertyName("pc")]
    public required string Pc { get; set; } = "";

    [JsonPropertyName("monitor")]
    public required string Monitor { get; set; } = "";

    [JsonPropertyName("tool")]
    public required string Tool { get; set; } = "";

    [JsonPropertyName("scanner")]
    public required string Scanner { get; set; } = "";

    [JsonPropertyName("tablet")]
    public required string Tablet { get; set; } = "";

    [JsonPropertyName("mouse")]
    public required string Mouse { get; set; } = "";

    [JsonPropertyName("printer")]
    public required string Printer { get; set; } = "";

    [JsonPropertyName("desktop")]
    public required string Desktop { get; set; } = "";

    [JsonPropertyName("music")]
    public required string Music { get; set; } = "";

    [JsonPropertyName("desk")]
    public required string Desk { get; set; } = "";

    [JsonPropertyName("chair")]
    public required string Chair { get; set; } = "";

    [JsonPropertyName("comment")]
    public required string Comment { get; set; } = "";
}
