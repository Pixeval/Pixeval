using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    public class IllustrationCommentsResponse
    {
        [JsonPropertyName("comments")]
        public IEnumerable<Comment>? Comments { get; set; }

        [JsonPropertyName("next_url")]
        public string? NextUrl { get; set; }

        public class Comment
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("comment")]
            public string? CommentContent { get; set; }

            [JsonPropertyName("date")]
            public DateTimeOffset Date { get; set; }

            [JsonPropertyName("user")]
            public User? User { get; set; }

            [JsonPropertyName("has_replies")]
            public bool HasReplies { get; set; }

            [JsonPropertyName("stamp")]
            public Stamp? Stamp { get; set; }
        }

        public class Stamp
        {
            [JsonPropertyName("stamp_id")]
            public long StampId { get; set; }

            [JsonPropertyName("stamp_url")]
            public string? StampUrl { get; set; }
        }

        public class User
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("account")]
            public string? Account { get; set; }

            [JsonPropertyName("profile_image_urls")]
            public ProfileImageUrls? ProfileImageUrls { get; set; }
        }
    }
}