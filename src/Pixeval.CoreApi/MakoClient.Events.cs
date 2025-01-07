// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi;

public partial class MakoClient
{
    public event EventHandler<Exception>? TokenRefreshedFailed;

    public event EventHandler<TokenUser>? TokenRefreshed;

    internal void OnTokenRefreshedFailed(Exception e)
    {
        TokenRefreshedFailed?.Invoke(this, e);
    }

    internal void OnTokenRefreshed(TokenUser user)
    {
        TokenRefreshed?.Invoke(this, user);
    }
}
