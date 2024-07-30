using System;
using System.Collections.Generic;

namespace MFractor.IOC
{
    /// <summary>
    /// Resolves parts exported via MEF.
    /// <para/>
    /// An <see cref="IExportResolver"/> is responsible for routing export and compose parts invocations into the MEF container(s) provided by the outer product.
    /// <para/>
    /// A <see cref="IExportResolver"/> implementation backs the functionality provided by the <see cref="Resolver"/>.
    /// <para/>
    /// To declare the <see cref="IExportResolver"/> available in the current app domain, use the <see cref="DeclareExportResolverAttribute"/>.
    /// </summary>
    public interface IExportResolver
    {
        /// <summary>
        /// Prepares the export resolver.
        /// <para/>
        /// Internal Use Only: Calling <see cref="Prepare"/> outside of the scope of MFractors startup routine will result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        void Prepare();

        /// <summary>
        /// Gets a <see cref="Lazy{T}"/> to lazily resolve <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Lazy<T> GetExport<T>();

        /// <summary>
        /// Gets a <see cref="Lazy{T}"/> to lazily resolve many instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Lazy<IEnumerable<T>> GetExports<T>();

        /// <summary>
        /// Gets the instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetExportedValue<T>();

        /// <summary>
        /// Gets all instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetExportedValues<T>();

        /// <summary>
        /// Gets the instance of <paramref name="type"/>
        /// </summary>s
        /// <param name="type"></param>
        /// <returns></returns>
        object GetExportedValue(Type type);

        /// <summary>
        /// Gets all instances of <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<object> GetExportedValues(Type type);

        /// <summary>
        /// Inspects the <paramref name="instance"/> for all properties that are marked with the <see cref="System.Composition.ImportAttribute"/> or <see cref="System.ComponentModel.Composition.ImportAttribute"/> attributes and resolves them.
        /// <para/>
        /// Use <see cref="ComposeParts(object)"/> to resolve an objects dependencies when that particular type cannot be exported into MEF (For example: UI widgets or IDE integration points).
        /// </summary>
        /// <param name="instance"></param>
        void ComposeParts(object instance);
    }
}
