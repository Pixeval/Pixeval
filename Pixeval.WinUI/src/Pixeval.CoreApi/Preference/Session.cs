using System;
using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Preference
{
    /// <summary>
    /// Contains all the user configurable
    /// </summary>
    [PublicAPI]
    public record Session
    {
        /// <summary>
        /// User name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Token expiration
        /// </summary>
        public DateTimeOffset ExpireIn { get; set; }

        /// <summary>
        /// Current access token
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Current refresh token
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Avatar
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// User id
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Account for login
        /// </summary>
        public string? Account { get; set; }

        /// <summary>
        /// Indicates current user is Pixiv Premium or not
        /// </summary>
        public bool IsPremium { get; set; }

        /// <summary>
        /// WebAPI cookie
        /// </summary>
        public string? Cookie { get; set; }

        public override string? ToString()
        {
            return this.ToJson();
        }

        public bool RefreshRequired()
        {
            return AccessToken.IsNullOrEmpty() || DateTime.Now >= ExpireIn;
        }
    }
}