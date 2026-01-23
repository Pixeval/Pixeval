// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Mako.Model;

namespace Pixeval.AppManagement;

public record LoginContext
{
    public string CurrentRefreshToken { get; set; } = "";

    public Dictionary<string, TokenUser> Users { get; set; } = [];
}
