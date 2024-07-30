using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFractor.Configuration;
using MFractor.IOC;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for working with MFractors <see cref="IConfigurable"/> interface.
    /// </summary>
    public static class ConfigurationDependencyHelper
    {
        static readonly Type configurableType = typeof(IConfigurable);

        /// <summary>
        /// For the given <paramref name="configurable"/>, resolve all 
        /// </summary>
        /// <returns>The dependencies.</returns>
        /// <param name="configurable">Configurable.</param>
        /// <param name="engine">Engine.</param>
        public static List<IConfigurable> ResolveDependencies(IConfigurable configurable, IConfigurationEngine engine)
        {
            var type = typeof(IConfigurable);

            var props = DependencyHelper.FindDependencies(configurable);

            var dependencies = new List<IConfigurable>();

            if (props != null && props.Any())
            {
                foreach (var p in props)
                {
                    if (type.IsAssignableFrom(p.PropertyType))
                    {
                        var dependency = ResolvePropertyImportToConfigurable(p, engine);

                        if (dependency != null)
                        {
                            p.SetValue(configurable, dependency);
                            dependencies.Add(dependency);
                        }
                    }
                }
            }

            return dependencies;
        }

        public static IConfigurable ResolvePropertyImportToConfigurable(PropertyInfo property, IConfigurationEngine engine)
        {
            var propertyType = property.PropertyType;

            var configurables = engine.GetConfigurablesOfType(propertyType);

            if (configurables != null && configurables.Any())
            {
                return configurables.FirstOrDefault();
            }

            return null;
        }
    }
}
