// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;
using WinUI3Utilities.Attributes;

namespace Pixeval.AppManagement;

[GenerateConstructor(CallParameterlessConstructor = true), CopyTo]
public partial record LoginContext()
{
    public string RefreshToken { get; set; } = "";

    public bool IsPremium { get; set; }

    public string UserName { get; set; } = "";

    public string Password { get; set; } = "";

    public bool LogoutExit { get; set; }
}
