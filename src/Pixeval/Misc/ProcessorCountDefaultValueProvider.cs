using System;

namespace Pixeval.Misc
{
    public class ProcessorCountDefaultValueProvider : IDefaultValueProvider
    {
        public object ProvideValue()
        {
            return Environment.ProcessorCount;
        }
    }
}