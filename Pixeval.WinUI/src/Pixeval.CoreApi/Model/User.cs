using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record User
    {
        private User()
        {
        }
        
        public string? Name { get; set; }
        
        public string? Id { get; set; }
        
        public bool IsFollowed { get; set; }
        
        public string? Avatar { get; set; }
        
        public string? Introduction { get; set; }
        
        public long Follows { get; set; }
        
        public bool IsPremium { get; set; }
        
        public IEnumerable<Illustration>? Thumbnails { get; set; }
        
        /// <summary>
        /// Instantiate and tries to cache an <see cref="User"/> to <see cref="MakoClient"/>'s cache,
        /// if the cache entry already exists, this method will returns the cached instance
        /// </summary>
        /// <remarks>
        /// !!! This is the ONLY way to directly instantiate an <see cref="User"/> !!!
        /// You cannot instantiate <see cref="User"/> at anywhere else because we need
        /// something like an unique registration to simplify the caching system
        /// </remarks>
        /// <param name="id">The ID here is used as the cache key</param>
        /// <param name="makoClient">The <see cref="MakoClient"/> that owns the cache</param>
        /// <param name="instantiationConfiguration">Used to configure the newly instantiated <see cref="User"/></param>
        internal static User GetOrInstantiateAndConfigureUserFromCache(string id, MakoClient makoClient, Action<User> instantiationConfiguration)
        {
            return makoClient.GetCached<User>(CacheType.User, id) ?? new User().Apply(u =>
            {
                instantiationConfiguration(u);
                makoClient.Cache<User>(CacheType.User, id, u);
            });
        }
    }
}