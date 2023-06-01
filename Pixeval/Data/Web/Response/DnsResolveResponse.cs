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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class DnsResolveResponse
    {
        [JsonProperty("Status")]
        public long Status { get; set; }

        [JsonProperty("TC")]
        public bool Tc { get; set; }

        [JsonProperty("RD")]
        public bool Rd { get; set; }

        [JsonProperty("RA")]
        public bool Ra { get; set; }

        [JsonProperty("AD")]
        public bool Ad { get; set; }

        [JsonProperty("CD")]
        public bool Cd { get; set; }

        [JsonProperty("Question")]
        public List<Question> Questions { get; set; }

        [JsonProperty("Answer")]
        public List<Answer> Answers { get; set; }

        public class Answer
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public long Type { get; set; }

            [JsonProperty("TTL")]
            public long Ttl { get; set; }

            [JsonProperty("data")]
            public string Data { get; set; }
        }

        public class Question
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public long Type { get; set; }
        }
    }
}