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

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pixeval.Data.Web.Response;
using Pixeval.Objects.Primitive;

namespace Pixeval.Persisting
{
    /// <summary>
    ///     A class which represents current session. etc.
    /// </summary>
    public class Session
    {
        public static Session Current;

        public string Name { get; set; }

        public DateTime ExpireIn { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string AvatarUrl { get; set; }

        public string Id { get; set; }

        public DateTime CookieCreation { get; set; }

        public bool IsPremium { get; set; }
        
        public string Cookie { get; set; }

        public static Session Parse(TokenResponse token)
        {
            var response = token.Response;
            return new Session
            {
                Name = response.User.Name,
                ExpireIn = DateTime.Now + TimeSpan.FromSeconds(response.ExpiresIn),
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AvatarUrl = response.User.ProfileImageUrls.Px170X170,
                Id = response.User.Id,
                CookieCreation = Current?.CookieCreation ?? default,
                IsPremium = token.Response.User.IsPremium,
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        public async Task Store()
        {
            await File.WriteAllTextAsync(Path.Combine(PixevalContext.ConfFolder, PixevalContext.ConfigurationFileName), ToString());
        }

        public static async Task Restore()
        {
            Current = ConfExists()
                ? (await File.ReadAllTextAsync(Path.Combine(PixevalContext.ConfFolder, PixevalContext.ConfigurationFileName), Encoding.UTF8)).FromJson<Session>()
                : null;
        }

        public static bool ConfExists()
        {
            var path = Path.Combine(PixevalContext.ConfFolder, PixevalContext.ConfigurationFileName);
            return File.Exists(path) && new FileInfo(path).Length != 0;
        }

        public static bool RefreshRequired(Session identity)
        {
            return identity == null || identity.AccessToken.IsNullOrEmpty() || DateTime.Now - identity.ExpireIn >= TimeSpan.FromMinutes(50);
        }

        public static void Clear()
        {
            if (File.Exists(Path.Combine(PixevalContext.ConfFolder, PixevalContext.ConfigurationFileName)))
            {
                File.Delete(Path.Combine(PixevalContext.ConfFolder, PixevalContext.ConfigurationFileName));
            }
            Current = new Session();
        }
    }
}