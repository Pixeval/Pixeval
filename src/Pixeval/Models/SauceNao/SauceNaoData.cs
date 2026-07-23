using System.Text.Json.Serialization;
using Misaki;
using Pixeval.Views.Viewers;

namespace Pixeval.Models.SauceNao;

public record SauceNaoData
{
    [JsonPropertyName("pixiv_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? PixivId { get; set; }

    [JsonPropertyName("danbooru_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? DanbooruId { get; set; }

    [JsonPropertyName("yandere_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? YandereId { get; set; }

    [JsonPropertyName("gelbooru_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? GelbooruId { get; set; }

    [JsonPropertyName("sankaku_id")]
    // 仅作兜底
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? SankakuId { get; set; }

    public IIdentityInfo? ToIdentityInfo()
    {
        if (PixivId is { } pId)
            return new SimpleIdentityInfo(pId.ToString(), IPlatformInfo.Pixiv);
        if (DanbooruId is { } dId)
            return new SimpleIdentityInfo(dId.ToString(), IPlatformInfo.Danbooru);
        if (YandereId is { } yId)
            return new SimpleIdentityInfo(yId.ToString(), IPlatformInfo.Yandere);
        if (GelbooruId is { } gId)
            return new SimpleIdentityInfo(gId.ToString(), IPlatformInfo.Gelbooru);
        if (SankakuId is { } sId)
            return new SimpleIdentityInfo(sId.ToString(), IPlatformInfo.Sankaku);
        return null;
    }
}
