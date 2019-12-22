namespace Pixeval.Data
{
    public interface IParser<out T>
    {
        T Parse();
    }
}