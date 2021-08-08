namespace Pixeval.Util
{
    public interface ILocalizedBox<out T>
    {
        public T Value { get; }

        public string LocalizedString { get; }
    }
}