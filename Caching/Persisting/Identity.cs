using System;
using Pzxlane.Data.Model.Web.Response;
using Pzxlane.Objects;

namespace Pzxlane.Caching.Persisting
{
    public class Identity
    {
        public static Identity Global = new Identity();

        public string Name { get; set; }

        public DateTime ExpireIn { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string AvatarUrl { get; set; }

        public string Id { get; set; }

        public string MailAddress { get; set; }

        public string Account { get; set; }

        public static Identity Parse(TokenResponse token)
        {
            var response = token.ToResponse;
            return new Identity
            {
                Name = response.User.Name,
                ExpireIn = DateTime.Now + TimeSpan.FromMilliseconds(response.ExpiresIn),
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                AvatarUrl = response.User.ProfileImageUrls.Px170X170,
                Id = response.User.Id.ToString(),
                MailAddress = response.User.MailAddress,
                Account = response.User.Account
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}