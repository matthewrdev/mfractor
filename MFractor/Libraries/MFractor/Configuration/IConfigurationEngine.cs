using System;
using System.Collections.Generic;

namespace MFractor.Configuration
{
    /// <summary>
    /// The configuration engine is one of MFractors core sub-systems.
    /// <para/>
    /// It exposes access to all configurable features.
    /// </summary>
    public interface IConfigurationEngine
    {
        /// <summary>
        /// Resolves an instance of the provided type.
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="type">Type.</param>
        /// <param name="id">The configuration id.</param>
        object Resolve(Type type, ConfigurationId id = null);

        /// <summary>
        /// Resolve an instance of <typeparamref name="TConfigurable"/>.
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="id">Identifier.</param>
        /// <typeparam name="TConfigurable">The 1st type parameter.</typeparam>
        TConfigurable Resolve<TConfigurable>(ConfigurationId id = null) where TConfigurable : class, IConfigurable;

        /// <summary>
        /// Resolves <typeparamref name="TConfigurable"/>, cast as <typeparamref name="TCastType"/>.
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="id">Identifier.</param>
        /// <typeparam name="TConfigurable">The 1st type parameter.</typeparam>
        /// <typeparam name="TCastType">The 2nd type parameter.</typeparam>
        TCastType Resolve<TConfigurable, TCastType>(ConfigurationId id = null) where TConfigurable : class, IConfigurable
                                                        where TCastType : class, TConfigurable, IConfigurable;

        /// <summary>
        /// Resolve all instances of <typeparamref name="TConfigurable"/>.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="id">Identifier.</param>
        /// <typeparam name="TConfigurable">The 1st type parameter.</typeparam>
        IEnumerable<TConfigurable> ResolveAll<TConfigurable>(ConfigurationId id = null) where TConfigurable : class, IConfigurable;

        /// <summary>
        /// Gets the configurable by the provided <paramref name="identifier"/>.
        /// </summary>
        /// <returns>The by identifier.</returns>
        /// <param name="identifier">Identifier.</param>
        IConfigurable GetByIdentifier(string identifier);

        /// <summary>
        /// Gets all configurables of <typeparamref name="TConfigurable"/> type.
        /// </summary>
        /// <returns>The configurables of type.</returns>
        /// <typeparam name="TConfigurable">The 1st type parameter.</typeparam>
        IEnumerable<TConfigurable> GetConfigurablesOfType<TConfigurable>() where TConfigurable : IConfigurable;

        /// <summary>
        /// Gets all the configurables in MFractor.
        /// </summary>
        /// <value>The configurables.</value>
        IEnumerable<IConfigurable> Configurables { get; }

        /// <summary>
        /// Does this 
        /// </summary>
        /// <returns>The contains.</returns>
        /// <param name="identifier">Identifier.</param>
        bool Contains(string identifier);

        /// <summary>
        /// Gets all configurables of the provided <paramref name="type"/>.
        /// </summary>
        /// <returns>The configurables of type.</returns>
        /// <param name="type">Type.</param>
        IEnumerable<IConfigurable> GetConfigurablesOfType(Type type);
    }
}
