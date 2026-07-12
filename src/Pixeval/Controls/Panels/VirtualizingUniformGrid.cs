// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Controls;

/// <summary>
/// A virtualizing uniform grid whose finite bounds determine an integer row and column capacity.
/// </summary>
public class VirtualizingUniformGrid : VirtualizingAdaptiveGrid
{
    /// <inheritdoc />
    protected override bool ConstrainToAvailableMajor => true;
}
