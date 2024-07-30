using System;
using System.Collections.Generic;
using MFractor.Configuration;
using MFractor.IOC;

namespace MFractor.Configuration
{
    /// <summary>
    /// A strongly type MEF part repository that collects all exported <typeparamref name="TConfigurable"/> and then lazily resolves them when first used.
    /// <para/>
    /// The <see cref="IConfigurablePartRepository{T}"/> is backed by the <see cref="IConfigurationEngine"/> instead of the standard resolver.
    /// <para/>
    /// Prefer using an <see cref="IConfigurablePartRepository{T}"/> instead of <see cref="System.ComponentModel.Composition.ImportManyAttribute"/> for performance reasons.
    /// </summary>
    /// <typeparam name="TConfigurable"></typeparam>
    public interface IConfigurablePartRepository<TConfigurable> : IPartRepository<TConfigurable> where TConfigurable : class, IConfigurable
    {
        /// <summary>
        /// The <see cref="TConfigurable"/> parts available.
        /// </summary>
        IReadOnlyList<TConfigurable> Configurables { get; }

        /// <summary>
        /// Gets the <typeparamref name="TPart"/> from the repository or null if it does not exist.
        /// </summary>
        /// <typeparam name="TPart"></typeparam>
        /// <returns></returns>
        TPart GetConfigurable<TPart>(ConfigurationId configurationId = null) where TPart : TConfigurable;
    }
}
