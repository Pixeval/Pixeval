namespace Pixeval.Objects
{
    public class ObjectJsonConverter : IConverter<object, string>
    {
        public string Convert(object input)
        {
            return input.ToJson();
        }
    }
}