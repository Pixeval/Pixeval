using System.Runtime.CompilerServices;

namespace Pixeval.CommunityToolkit.Predicates
{
    /// <summary>
    /// An <see cref="IPredicate{T}"/> type matching all instances of a given type.
    /// </summary>
    /// <typeparam name="T">The type of items to match.</typeparam>
    internal readonly struct PredicateByAny<T> : IPredicate<T>
        where T : class
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(T element)
        {
            return true;
        }
    }
}