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

using Refit;

namespace Pixeval.Data.Web.Request
{
    public class DnsResolveRequest
    {
        [AliasAs("ct")]
        public string Ct { get; set; }

        [AliasAs("name")]
        public string Name { get; set; }

        [AliasAs("type")]
        public string Type { get; set; }

        [AliasAs("do")]
        public string Do { get; set; }

        [AliasAs("cd")]
        public string Cd { get; set; }
    }
}