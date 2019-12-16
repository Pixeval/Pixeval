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
using Refit;

namespace Pixeval.Data.Web.Request
{
    public class TokenRequest
    {
        [AliasAs("username")]
        public string Name { get; set; }

        [AliasAs("password")]
        public string Password { get; set; }

        [AliasAs("grant_type")]
        public string GrantType => "password";

        [AliasAs("client_id")]
        public string ClientId => "MOBrBDS8blbauoSck0ZfDbtuzpyT";

        [AliasAs("client_secret")]
        public string ClientSecret => "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";

        [AliasAs("get_secure_url")]
        public string GetSecureUrl => "1";
    }
}