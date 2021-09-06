using Refit;

namespace Pixeval.CoreApi.Net.Request
{
    public class PostIllustrationCommentRequest
    {
        [AliasAs("illust_id")]
        public string? IllustrationId { get; set; }

        [AliasAs("comment")]
        public string? CommentContent { get; set; }

        [AliasAs("parent_comment_id")]
        public string? ParentCommentId { get; set; }
    }
}