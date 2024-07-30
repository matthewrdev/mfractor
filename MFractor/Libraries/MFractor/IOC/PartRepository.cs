using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.IOC
{
    /// <summary>
    /// The base class for an implementation of <see cref="IPartRepository{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PartRepository<T> : IPartRepository<T> where T : class
    {
        readonly Lazy<IReadOnlyList<T>> parts;
        public IReadOnlyList<T> Parts => parts.Value;

        readonly Lazy<IPartResolver> partResolver;
        public IPartResolver PartResolver => partResolver.Value;

        protected PartRepository(Lazy<IPartResolver> partResolver)
        {
            this.partResolver = partResolver;
            this.parts = new Lazy<IReadOnlyList<T>>(() => PartResolver.ResolveAll<T>().ToList());
        }

        public TPart GetPart<TPart>() where TPart : T
        {
            return Parts.OfType<TPart>().FirstOrDefault();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Parts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parts.GetEnumerator();
        }
    }
}