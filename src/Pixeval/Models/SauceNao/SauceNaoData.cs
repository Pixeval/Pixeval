using System.Text.Json.Serialization;
using Misaki;
using Pixeval.Views.Viewers;

namespace Pixeval.Models.SauceNao;

public record SauceNaoData
{
    [JsonPropertyName("pixiv_id")]
    public long? PixivId { get; set; }

    [JsonPropertyName("danbooru_id")]
    public string? DanbooruId { get; set; }

    [JsonPropertyName("yandere_id")]
    public string? YandereId { get; set; }

    [JsonPropertyName("gelbooru_id")]
    public string? GelbooruId { get; set; }

    [JsonPropertyName("sankaku_id")]
    public string? SankakuId { get; set; }

    public IIdentityInfo? ToIdentityInfo()
    {
        if (PixivId is { } id)
            return new SimpleIdentityInfo(id.ToString(), IPlatformInfo.Pixiv);
        if (DanbooruId is not null)
            return new SimpleIdentityInfo(DanbooruId, IPlatformInfo.Danbooru);
        if (YandereId is not null)
            return new SimpleIdentityInfo(YandereId, IPlatformInfo.Yandere);
        if (GelbooruId is not null)
            return new SimpleIdentityInfo(GelbooruId, IPlatformInfo.Gelbooru);
        if (SankakuId is not null)
            return new SimpleIdentityInfo(SankakuId, IPlatformInfo.Sankaku);
        return null;
    }
}
