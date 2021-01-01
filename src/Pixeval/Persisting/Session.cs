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
        /// <summary>
        ///     Global session, contains both app-API and web-API
        /// </summary>
        public static Session Current;

        /// <summary>
        ///     User's display name of Pixiv
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     AccessToken expiration
        /// </summary>
        public DateTime ExpireIn { get; set; }

        /// <summary>
        ///     The OAuth AccessToken of the session, which is the most important field
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        ///     The OAuth RefreshToken of the session, which is used to refresh the session
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        ///     Avatar url of the current user
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        ///     Pixiv ID of current user
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     MailAddress of current user, it is also the login account, managed by CredentialManager
        /// </summary>
        [JsonIgnore]
        public string MailAddress { get; set; }

        /// <summary>
        ///     Account of current user, be aware not to mix it up with MailAddress
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        ///     Password of current user, managed by CredentialManager
        /// </summary>
        [JsonIgnore]
        public string Password { get; set; }

        /// <summary>
        ///     A.k.a PHPSESSID(Cookie), used by web-API
        /// </summary>
        public string PhpSessionId { get; set; }

        /// <summary>
        ///     The creation date of the PhpSessionId
        /// </summary>
        public DateTime CookieCreation { get; set; }

        /// <summary>
        ///     Indicate whether the accounts is pixiv premium
        /// </summary>
        public bool IsPremium { get; set; }

        /// <summary>
        ///     Parse a <see cref="TokenResponse" /> to an <see cref="Session" />
        /// </summary>
        /// <param name="password">password</param>
        /// <param name="token">to be parsed</param>
        /// <returns></returns>
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
                PhpSessionId = Current?.PhpSessionId,
                CookieCreation = Current?.CookieCreation ?? default,
                IsPremium = token.ToResponse.User.IsPremium
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        /// <summary>
        ///     Save the identity object to local
        /// </summary>
        /// <returns></returns>
        public async Task Store()
        {
            await File.WriteAllTextAsync(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName), ToString());
            CredentialManager.SaveCredentials(AppContext.AppIdentifier, new NetworkCredential(MailAddress, Password));
        }

        /// <summary>
        ///     Load the identity object from local
        /// </summary>
        /// <returns></returns>
        public static async Task Restore()
        {
            Current = (await File.ReadAllTextAsync(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName), Encoding.UTF8)).FromJson<Session>();
            var credential = CredentialManager.GetCredentials(AppContext.AppIdentifier);
            Current.MailAddress = credential.UserName;
            Current.Password = credential.Password;
        }

        /// <summary>
        ///     Inspect the configuration file are present
        /// </summary>
        /// <returns></returns>
        public static bool ConfExists()
        {
            var path = Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName);
            return File.Exists(path) && new FileInfo(path).Length != 0 && CredentialManager.GetCredentials(AppContext.AppIdentifier) != null;
        }

        /// <summary>
        ///     Check if the app-API session need to be refresh, we just simply compare two <see cref="DateTime" />
        /// </summary>
        /// <param name="identity">app-API session</param>
        /// <returns>true if needed</returns>
        public static bool AppApiRefreshRequired(Session identity)
        {
            return identity == null || identity.AccessToken.IsNullOrEmpty() || identity.ExpireIn == default || identity.ExpireIn <= DateTime.Now;
        }

        /// <summary>
        ///     Check if the web-Api session need to be refresh, the refreshing policy checks if the cookie has
        ///     been created for 7 days
        /// </summary>
        /// <param name="identity">web-API session</param>
        /// <returns>true if needed</returns>
        public static bool WebApiRefreshRequired(Session identity)
        {
            return identity == null || identity.PhpSessionId.IsNullOrEmpty() || identity.CookieCreation == default || (DateTime.Now - identity.CookieCreation).Days >= 7;
        }

        /// <summary>
        ///     Refresh the session if required, it is composed of two sections, app-API login
        ///     and web-API login, each section has its own expiration check, and both two sections
        ///     are invoked separately, we will only refresh the required section
        /// </summary>
        /// <returns></returns>
        public static async Task RefreshIfRequired()
        {
            if (Current == null) await Restore();

            // refresh through app-API
            static async Task RefreshAppApi()
            {
                if (AppApiRefreshRequired(Current))
                {
                    // we'd prefer use refresh token
                    if (Current?.RefreshToken.IsNullOrEmpty() is true)
                        await Authentication.AppApiAuthenticate(Current?.MailAddress, Current?.Password);
                    else
                        await Authentication.AppApiAuthenticate(Current?.RefreshToken);
                }
            }

            //// refresh through web-API
            //static async Task RefreshWebApi()
            //{
            //    if (WebApiRefreshRequired(Current)) await Authentication.WebApiAuthenticate(Current?.MailAddress, Current?.Password);
            //}

            // wait for both two sections to be complete
            await Task.WhenAll(RefreshAppApi()
                //, RefreshWebApi()
                );
        }

        /// <summary>
        ///     clear the session and user identity, this action will remove the persisting data
        /// </summary>
        public static void Clear()
        {
            if (File.Exists(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName))) File.Delete(Path.Combine(AppContext.ConfFolder, AppContext.ConfigurationFileName));
            if (CredentialManager.GetCredentials(AppContext.AppIdentifier) != null) CredentialManager.RemoveCredentials(AppContext.AppIdentifier);
            Current = new Session();
        }
    }
}