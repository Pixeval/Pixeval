using System.Collections.Generic;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record SpotlightDetail
    {
        public SpotlightArticle SpotlightArticle { get; set; }
        
        public string Introduction { get; set; }
        
        public IEnumerable<Illustration> Illustrations { get; set; }

        public SpotlightDetail(SpotlightArticle spotlightArticle, string introduction, IEnumerable<Illustration> illustrations)
        {
            SpotlightArticle = spotlightArticle;
            Introduction = introduction;
            Illustrations = illustrations;
        }
    }
}