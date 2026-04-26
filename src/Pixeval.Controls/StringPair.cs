// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;

namespace Pixeval.Controls;

public record StringPair(object Value, string Description) : IReadOnlyStringPair<object>
{
    /// <inheritdoc />
    public override string ToString() => Description;
}
