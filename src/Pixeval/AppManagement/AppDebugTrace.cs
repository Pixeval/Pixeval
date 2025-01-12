// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using WinUI3Utilities.Attributes;

namespace Pixeval.AppManagement;

[GenerateConstructor]
public partial record AppDebugTrace
{
    public AppDebugTrace()
    {
    }

    public bool ExitedSuccessfully { get; set; } = true;
}
