using System;
using System.Runtime.CompilerServices;

namespace Pixeval.CommunityToolkit.Predicates
{
    /// <summary>
    /// An <see cref="IPredicate{T}"/> type matching items of a given type.
    /// </summary>
    /// <typeparam name="T">The type of items to match.</typeparam>
    /// <typeparam name="TState">The type of state to use when matching items.</typeparam>
    internal readonly struct PredicateByFunc<T, TState> : IPredicate<T>
        where T : class
    {
        /// <summary>
        /// The state to give as input to <see name="predicate"/>.
        /// </summary>
        private readonly TState state;

        /// <summary>
        /// The predicate to use to match items.
        /// </summary>
        private readonly Func<T, TState, bool> predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateByFunc{T, TState}"/> struct.
        /// </summary>
        /// <param name="state">The state to give as input to <paramref name="predicate"/>.</param>
        /// <param name="predicate">The predicate to use to match items.</param>
        public PredicateByFunc(TState state, Func<T, TState, bool> predicate)
        {
            this.state = state;
            this.predicate = predicate;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(T element)
        {
            return predicate(element, state);
        }
    }

    /// <summary>
    /// An <see cref="IPredicate{T}"/> type matching items of a given type.
    /// </summary>
    /// <typeparam name="T">The type of items to match.</typeparam>
    internal readonly struct PredicateByFunc<T> : IPredicate<T>
        where T : class
    {
        /// <summary>
        /// The predicate to use to match items.
        /// </summary>
        private readonly Func<T, bool> predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateByFunc{T}"/> struct.
        /// </summary>
        /// <param name="predicate">The predicate to use to match items.</param>
        public PredicateByFunc(Func<T, bool> predicate)
        {
            this.predicate = predicate;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(T element)
        {
            return predicate(element);
        }
    }
}