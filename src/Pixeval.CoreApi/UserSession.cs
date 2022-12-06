using System;

namespace Pixeval.CoreApi
{
    public record UserSession(string UserId, string RefreshToken, string AccessToken, DateTimeOffset Updated);
}
