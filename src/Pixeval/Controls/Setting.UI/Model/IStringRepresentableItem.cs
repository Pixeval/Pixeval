namespace Pixeval.Controls.Setting.UI.Model
{
    public interface IStringRepresentableItem
    {
        object Item { get; }

        string StringRepresentation { get; }
    }
}