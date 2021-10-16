using System;

namespace Pixeval.Misc
{
    public class MinDateTimeOffSetDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return DateTimeOffset.MinValue;
        }
    }

    public class MaxDateTimeOffSetDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return DateTimeOffset.MaxValue;
        }
    }

    public class CurrentDateTimeOffSetDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return DateTimeOffset.Now;
        }
    }

    public class DecrementedDateTimeOffSetDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return DateTimeOffset.Now - TimeSpan.FromDays(1);
        }
    }
}