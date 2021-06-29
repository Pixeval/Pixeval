using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record Illustration
    {
        private Illustration()
        {
            
        }
        
        public string? Id { get; set; }

        public bool IsUgoira { get; set; }

        public string? OriginalUrl { get; set; }

        public string? LargeUrl { get; set; }

        public string? ThumbnailUrl { get; set; }

        public int Bookmarks { get; set; }

        public bool IsBookmarked { get; set; }

        public bool IsManga { get; set; }

        public string? Title { get; set; }

        public string? ArtistName { get; set; }

        public string? ArtistId { get; set; }

#if DEBUG // this object may will serialized into json under DEBUG mode, use [JsonIgnore] to prevents the object cycle on this property
        [JsonIgnore]
#endif
        public Illustration[]? MangaMetadata { get; set; }

        public DateTimeOffset PublishDate { get; set; }

        public int TotalViews { get; set; }

        public Resolution? Resolution { get; set; }
        
        public IEnumerable<Tag>? Tags { get; set; }

        public bool IsR18 => Tags?.Any(x => Regex.IsMatch(x.Name ?? string.Empty, "[Rr][-]?18[Gg]?") || Regex.IsMatch(x.TranslatedName ?? string.Empty, "[Rr][-]?18[Gg]?")) ?? false;

        public void SetBookmark()
        {
            IsBookmarked = true;
        }

        public void UnsetBookmark()
        {
            IsBookmarked = false;
        }

        /// <summary>
        /// Instantiate and tries to cache an <see cref="Illustration"/> to <see cref="MakoClient"/>'s cache,
        /// if the cache entry already exists, this method will returns the cached instance
        /// </summary>
        /// <remarks>
        /// !!! This is the ONLY way to directly instantiate an <see cref="Illustration"/> !!!
        /// You cannot instantiate <see cref="Illustration"/> at anywhere else because we need
        /// something like an unique registration to simplify the caching system
        /// </remarks>
        /// <param name="id">The ID here is used as the cache key</param>
        /// <param name="makoClient">The <see cref="MakoClient"/> that owns the cache</param>
        /// <param name="instantiationConfiguration">Used to configure the newly instantiated <see cref="Illustration"/></param>
        internal static Illustration GetOrInstantiateAndConfigureIllustrationFromCache(string id, MakoClient makoClient, Action<Illustration> instantiationConfiguration)
        {
            return makoClient.GetCached<Illustration>(CacheType.Illustration, id) ?? new Illustration().Apply(i =>
            {
                instantiationConfiguration(i);
                makoClient.Cache<Illustration>(CacheType.Illustration, id, i);
            });
        }
    }
}