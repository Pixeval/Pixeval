using System;

namespace Pixeval.Misc
{
    public class DateTimeOffSetDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return DateTimeOffset.MinValue;
        }
    }
}