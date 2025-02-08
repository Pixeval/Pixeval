// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Attributes;
using WinUI3Utilities.Attributes;

namespace Pixeval.AppManagement;

[GenerateConstructor(CallParameterlessConstructor = true), CopyTo]
public partial record AppDebugTrace()
{
    public bool ExitedSuccessfully { get; set; } = true;
}
