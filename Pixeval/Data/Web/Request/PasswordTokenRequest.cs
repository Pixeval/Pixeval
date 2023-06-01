#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

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

using Refit;

namespace Pixeval.Data.Web.Request
{
    public class PasswordTokenRequest
    {
        [AliasAs("client_id")]
        public string ClientId => "MOBrBDS8blbauoSck0ZfDbtuzpyT";

        [AliasAs("client_secret")]
        public string ClientSecret => "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";

        [AliasAs("code")]
        public string Code { get; set; }

        [AliasAs("grant_type")]
        public string GrantType => "authorization_code";

        [AliasAs("include_policy")]
        public string IncludePolicy => "true";

        [AliasAs("code_verifier")]
        public string CodeVerifier { get; set; }

        [AliasAs("redirect_uri")]
        public string RedirectUri => "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback";
    }
}