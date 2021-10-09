namespace Pixeval.Misc
{
    public class AppWidthDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return App.PredetermineEstimatedWindowSize().Item1;
        }
    }

    public class AppHeightDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return App.PredetermineEstimatedWindowSize().Item2;
        }
    }
}