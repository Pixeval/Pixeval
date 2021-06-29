using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Preference
{
    /// <summary>
    /// Contains all the user-configurable keys
    /// </summary>
    [PublicAPI]
    public class MakoClientConfiguration
    {
        public MakoClientConfiguration()
        {
            CultureInfo = CultureInfo.CurrentCulture;
            ConnectionTimeout = 5000;
            Bypass = false;
            MirrorHost = null;
            ExcludeTags = null;
            IncludeTags = null;
            MinBookmark = 0;
            AllowCache = false;
            CacheEntrySlidingExpiration = TimeSpan.FromMinutes(5);
        }

        public MakoClientConfiguration(int connectionTimeout, bool bypass, string? mirrorHost, ISet<string>? excludeTags, ISet<string>? includeTags, int minBookmark, bool allowCache, TimeSpan cacheEntrySlidingExpiration, CultureInfo cultureInfo)
        {
            ConnectionTimeout = connectionTimeout;
            Bypass = bypass;
            MirrorHost = mirrorHost;
            ExcludeTags = excludeTags;
            IncludeTags = includeTags;
            MinBookmark = minBookmark;
            AllowCache = allowCache;
            CacheEntrySlidingExpiration = cacheEntrySlidingExpiration;
            CultureInfo = cultureInfo;
        }

        public CultureInfo CultureInfo { get; set; }
        
        public int ConnectionTimeout { get; set; }
        
        /// <summary>
        /// Automatically bypass GFW or not, default is set to true.
        /// If you are currently living in China Mainland, turn it on to make sure
        /// you can use Mako without using any kind of proxy, otherwise you will
        /// need a proper proxy server to bypass the GFW
        /// </summary>
        public bool Bypass { get; set; }

        /// <summary>
        /// Mirror server's host of image downloading
        /// </summary>
        public string? MirrorHost { get; set; }

        /// <summary>
        /// Indicates which tags should be strictly exclude when performing a query operation
        /// </summary>
        public ISet<string>? ExcludeTags { get; }

        /// <summary>
        /// Indicates which tags should be strictly include when performing a query operation
        /// </summary>
        public ISet<string>? IncludeTags { get; }

        /// <summary>
        /// Any illust with less bookmarks will be filtered out
        /// </summary>
        public int MinBookmark { get; set; }
        
        public bool AllowCache { get; set; }
        
        public TimeSpan CacheEntrySlidingExpiration { get; set; }

        public MakoClientConfiguration UseCache()
        {
            AllowCache = true;
            return this;
        }

        public MakoClientConfiguration UseBypass()
        {
            Bypass = true;
            return this;
        }
    }
}