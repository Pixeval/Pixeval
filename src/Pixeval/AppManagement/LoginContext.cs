// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using WinUI3Utilities.Attributes;

namespace Pixeval.AppManagement;

[GenerateConstructor]
public partial record LoginContext
{
    public LoginContext()
    {
    }

    public string RefreshToken { get; set; } = "";

    public string UserName { get; set; } = "";

    public string Password { get; set; } = "";

    public bool LogoutExit { get; set; }
}
