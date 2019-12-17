// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class QueryResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public List<Response> ToResponse { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("pagination")]
        public Pagination QueryPagination { get; set; }

        public class Pagination
        {
            [JsonProperty("pages")]
            public long Pages { get; set; }
        }

        public class Response
        {
            [JsonProperty("id")]
            public long Id { get; set; }
        }
    }
}