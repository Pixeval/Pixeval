using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class PixivUserResponse
    {
        [JsonPropertyName("user_previews")]
        public IEnumerable<UserEssential.User>? Users { get; set; }
        
        [JsonPropertyName("next_url")]
        public string? NextUrl { get; set; }
    }
}