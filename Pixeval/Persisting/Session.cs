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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public string MailAddress { get; set; }

        public string Account { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        public DateTime CookieCreation { get; set; }

        public bool IsPremium { get; set; }

        public static Session Parse(string password, TokenResponse token)
        {
            var response = token.ToResponse;
            return new Session
            {
                Name = response.User.Name,
                ExpireIn = DateTime.Now + TimeSpan.FromSeconds(response.ExpiresIn),
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AvatarUrl = response.User.ProfileImageUrls.Px170X170,
                Id = response.User.Id.ToString(),
                MailAddress = response.User.MailAddress,
                Account = response.User.Account,
                Password = password,
                CookieCreation = Current?.CookieCreation ?? default,
                IsPremium = token.ToResponse.User.IsPremium
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        public async Task Store()
        {
            await File.WriteAllTextAsync(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName), ToString());
            CredentialManager.SaveCredentials(AppContext.AppIdentifier, new NetworkCredential(MailAddress, Password));
        }

        public static async Task Restore()
        {
            Current = (await File.ReadAllTextAsync(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName), Encoding.UTF8)).FromJson<Session>();
            var credential = CredentialManager.GetCredentials(AppContext.AppIdentifier);
            Current.MailAddress = credential.UserName;
            Current.Password = credential.Password;
        }

        public static bool ConfExists()
        {
            var path = Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName);
            return File.Exists(path) && new FileInfo(path).Length != 0 && CredentialManager.GetCredentials(AppContext.AppIdentifier) != null;
        }

        public static bool AppApiRefreshRequired(Session identity)
        {
            return identity == null || identity.AccessToken.IsNullOrEmpty() || identity.ExpireIn == default || identity.ExpireIn <= DateTime.Now;
        }

        public static async Task RefreshIfRequired()
        {
            if (Current == null)
            {
                await Restore();
            }

            if (AppApiRefreshRequired(Current))
            {
                if (Current?.RefreshToken.IsNullOrEmpty() is true)
                {
                    await Authentication.AppApiAuthenticate(Current?.MailAddress, Current?.Password);
                }
                else
                {
                    await Authentication.AppApiAuthenticate(Current?.RefreshToken);
                }
            }
        }

        public static void Clear()
        {
            if (File.Exists(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName)))
            {
                File.Delete(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName));
            }
            if (CredentialManager.GetCredentials(AppContext.AppIdentifier) != null)
            {
                CredentialManager.RemoveCredentials(AppContext.AppIdentifier);
            }
            Current = new Session();
        }
    }
}