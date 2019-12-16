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
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pixeval.Data.ViewModel;

namespace Pixeval.Data.Web.Response
{
    public class SpotlightResponse
    {
        [JsonProperty("spotlight_articles")]
        public List<SpotlightArticle> SpotlightArticles { get; set; }

        [JsonProperty("next_url")]
        public Uri NextUrl { get; set; }
    }
}