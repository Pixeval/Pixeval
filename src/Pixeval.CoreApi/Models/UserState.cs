using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;

namespace Pixeval.CoreApi.Models
{
    public record UserState(
        [property: JsonPropertyName("can_change_pixiv_id")] bool CanChangePixivId,
        [property: JsonPropertyName("has_changed_pixiv_id")] bool HasChangedPixivId,
        [property: JsonPropertyName("has_password")] bool HasPassword,
        [property: JsonPropertyName("is_mail_authorized")] bool IsMailAuthorized);
}
