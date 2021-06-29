using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    internal class PixivSingleIllustResponse
    {
        [JsonPropertyName("illust")]
        public IllustrationEssential.Illust? Illust { get; set; }
    }
}