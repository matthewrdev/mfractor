using System;
using System.Collections.Generic;

namespace MFractor.IOC
{
    /// <summary>
    /// A MEF part that provides part resolution for an <see cref="IPartRepository{T}"/>.
    /// <para/>
    /// The <see cref="IPartResolver"/> allows constructor injection to resolve parts for <see cref="IPartRepository{T}"/> implementations, decoupling it from any particular part resolution methodology.
    /// </summary>
    public interface IPartResolver
    {
        /// <summary>
        /// Resolve all parts of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> ResolveAll<T>() where T : class;
    }
}
