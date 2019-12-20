// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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
using System;
using Newtonsoft.Json;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Data.Web.Response
{
    public class TokenResponse
    {
        [JsonProperty("response")]
        public Response ToResponse { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }

        public class Response
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]

            public long ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("device_token")]
            public string DeviceToken { get; set; }
        }

        public class User
        {
            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls ProfileImageUrls { get; set; }

            [JsonProperty("id")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("mail_address")]
            public string MailAddress { get; set; }

            [JsonProperty("is_premium")]
            public bool IsPremium { get; set; }

            [JsonProperty("x_restrict")]
            public long XRestrict { get; set; }

            [JsonProperty("is_mail_authorized")]
            public bool IsMailAuthorized { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonProperty("px_16x16")]
            public string Px16X16 { get; set; }

            [JsonProperty("px_50x50")]
            public string Px50X50 { get; set; }

            [JsonProperty("px_170x170")]
            public string Px170X170 { get; set; }
        }

        private class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t)
            {
                return t == typeof(long) || t == typeof(long?);
            }

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (long.TryParse(value, out var l)) return l;
                throw new TypeMismatchException("Cannot unmarshal type long");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }

                var value = (long) untypedValue;
                serializer.Serialize(writer, value.ToString());
            }
        }
    }
}