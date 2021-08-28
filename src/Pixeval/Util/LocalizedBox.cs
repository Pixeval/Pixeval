using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Util
{
    public interface ILocalizedBox<T>
    {
        public T Value { get; }

        public string LocalizedString { get; }

        IEnumerable<ILocalizedBox<T>> GetAllOptions();

        ILocalizedBox<T> GetOptionOf(T value)
        {
            return GetAllOptions().First(option => option.Value!.Equals(value));
        }
    }
}