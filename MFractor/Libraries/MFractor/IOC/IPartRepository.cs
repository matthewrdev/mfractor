using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MFractor.IOC
{
    /// <summary>
    /// A strongly type MEF part repository that collects all exported <typeparamref name="T"/> and then lazily resolves them when <see cref="Parts"/> is first used.
    /// <para/>
    /// Where possible, prefer using an <see cref="IPartRepository{T}"/> instead of the <see cref="ImportManyAttribute"/>.
    /// <para/>
    /// Lazily resolving all parts is considerably more performant than using the <see cref="ImportManyAttribute"/> as it defers the instantiation cost of individual parts until <see cref="Parts"/> first use.
    /// </summary>
    /// <typeparam name="T">The part type to resolve</typeparam>
    public interface IPartRepository<T> : IEnumerable<T> where T : class
    {
        /// <summary>
        /// The <see cref="T"/> parts available within the app domain.
        /// </summary>
        IReadOnlyList<T> Parts { get; }

        /// <summary>
        /// Gets the <typeparamref name="TPart"/> from the repository or null if it does not exist.
        /// </summary>
        TPart GetPart<TPart>() where TPart : T;
    }
}
