using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MFractor.IOC
{
    /// <summary>
    /// The base class of an <see cref="IExportResolver"/> implementation that provides an implementation for <see cref="ComposeParts(object)"/>.
    /// <para/>
    /// A <see cref="BaseExportResolver"/> is responsible for routing <see cref="IExportResolver.GetExport{T}"/> and <see cref="IExportResolver.ComposeParts(object)"/> invocations into the MEF container provided by the outer product.
    /// </summary>
    public abstract class BaseExportResolver : IExportResolver
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        /// <summary>
        /// Inspects the <paramref name="instance"/> for all properties that are marked with the <see cref="System.Composition.ImportAttribute"/> or <see cref="System.ComponentModel.Composition.ImportAttribute"/> attributes and resolves them.
        /// <para/>
        /// Use <see cref="IExportResolver.ComposeParts(object)"/> to resolve an objects dependencies when it cannot be exported into MEF (EG: Controls, IDE integration points).
        /// </summary>
        public virtual void ComposeParts(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var dependencies = DependencyHelper.FindDependencies(instance);

            foreach (var dep in dependencies)
            {
                try
                {
                    var value = GetExportedValue(dep.PropertyType);
                    dep.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                    log?.Info("Failed to set " + dep.Name + " onto " + instance.GetType() + " (" + dep.PropertyType + ").");
                    Debugger.Break();
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Lazy{T}"/> to lazily resolve <typeparamref name="T"/>.
        /// </summary>
        public abstract Lazy<T> GetExport<T>();

        /// <summary>
        /// Gets the instance of <typeparamref name="T"/>.
        /// </summary>
        public abstract T GetExportedValue<T>();

        /// <summary>
        /// Gets the instance of <paramref name="type"/>
        /// </summary>
        public abstract object GetExportedValue(Type type);

        /// <summary>
        /// Gets all instances of <typeparamref name="T"/>.
        /// </summary>
        public abstract IEnumerable<T> GetExportedValues<T>();

        /// <summary>
        /// Gets all instances of <paramref name="type"/>
        /// </summary>
        public abstract IEnumerable<object> GetExportedValues(Type type);

        /// <summary>
        /// Gets a <see cref="Lazy{T}"/> to lazily resolve many instances of <typeparamref name="T"/>.
        /// </summary>
        public abstract Lazy<IEnumerable<T>> GetExports<T>();

        /// <summary>
        /// Internal Use Only
        /// <para/>
        /// Prepares the export resolver.
        /// <para/>
        /// Calling <see cref="Prepare"/> outside of the scope of MFractors startup routine will result in an exception.
        /// </summary>
        public virtual void Prepare()
        {
        }
    }
}
