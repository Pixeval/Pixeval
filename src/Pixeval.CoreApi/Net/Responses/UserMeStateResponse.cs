using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Pixeval.CoreApi.Models;

namespace Pixeval.CoreApi.Net.Responses
{
    public record UserMeStateResponse(
        [property: JsonPropertyName("user_state")] UserState UserState);
}
