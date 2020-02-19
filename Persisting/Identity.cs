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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;

namespace Pixeval.Persisting
{
    public class Identity
    {
        public static Identity Global;

        public string Name { get; set; }

        public DateTime ExpireIn { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string AvatarUrl { get; set; }

        public string Id { get; set; }

        public string MailAddress { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public static Identity Parse(string password, TokenResponse token)
        {
            var response = token.ToResponse;
            return new Identity
            {
                Name = response.User.Name,
                ExpireIn = DateTime.Now + TimeSpan.FromSeconds(response.ExpiresIn),
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AvatarUrl = response.User.ProfileImageUrls.Px170X170,
                Id = response.User.Id.ToString(),
                MailAddress = response.User.MailAddress,
                Account = response.User.Account,
                Password = password
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        public async Task Store()
        {
            await File.WriteAllTextAsync(Path.Combine(PixevalEnvironment.ConfFolder, PixevalEnvironment.ConfigurationFileName), ToString());
        }

        public static async Task Restore()
        {
            Global = (await File.ReadAllTextAsync(Path.Combine(PixevalEnvironment.ConfFolder, PixevalEnvironment.ConfigurationFileName), Encoding.UTF8)).FromJson<Identity>();
        }

        public static bool ConfExists()
        {
            var path = Path.Combine(PixevalEnvironment.ConfFolder, PixevalEnvironment.ConfigurationFileName);
            return File.Exists(path) && new FileInfo(path).Length != 0;
        }

        public static async ValueTask<bool> RefreshRequired()
        {
            return (await File.ReadAllTextAsync(Path.Combine(PixevalEnvironment.ConfFolder, PixevalEnvironment.ConfigurationFileName), Encoding.UTF8)).FromJson<Identity>().ExpireIn <= DateTime.Now;
        }

        public static async Task RefreshIfRequired()
        {
            if (Global == null) await Restore();

            if (await RefreshRequired()) await Authentication.Authenticate(Global?.MailAddress, Global?.Password);
        }

        public static void Clear()
        {
            File.Delete(Path.Combine(PixevalEnvironment.ConfFolder, PixevalEnvironment.ConfigurationFileName));
            Global = new Identity();
        }
    }
}