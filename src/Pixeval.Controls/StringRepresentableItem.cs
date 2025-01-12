// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

namespace Pixeval.Controls;

public record StringRepresentableItem(object Item, string StringRepresentation)
{
    public virtual bool Equals(StringRepresentableItem? other)
    {
        return other is not null && (ReferenceEquals(this, other) || Item.Equals(other.Item));
    }

    public override int GetHashCode() => Item.GetHashCode();

    public override string ToString() => StringRepresentation;
}
