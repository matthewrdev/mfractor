using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFractor.IOC
{
    /// <summary>
    /// A helper class for locating all properties that are marked with the <see cref="System.Composition.ImportAttribute"/> or the <see cref="System.ComponentModel.Composition.ImportAttribute"/>.
    /// </summary>
    public static class DependencyHelper
    {
        /// <summary>
        /// Finds any propertie with a getter and setter that are marked with marked with the <see cref="System.Composition.ImportAttribute"/> or the <see cref="System.ComponentModel.Composition.ImportAttribute"/>.
        /// </summary>
        /// <returns>The dependencies.</returns>
        /// <param name="instance">The object to find the depende.</param>
        public static IEnumerable<PropertyInfo> FindDependencies(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var type = instance.GetType();

            var properties = GetProperties(type);

            var props = properties.Where(p => Attribute.IsDefined(p, typeof(System.Composition.ImportAttribute)) || Attribute.IsDefined(p, typeof(System.ComponentModel.Composition.ImportAttribute)));

            var importedProperties = new List<PropertyInfo>();
            foreach (var p in props)
            {
                importedProperties.Add(p);
            }

            return importedProperties;
        }

        /// <summary>
        /// Gets all public instance properties for the given <paramref name="type"/>.
        /// </summary>
        /// <returns>The public properties.</returns>
        /// <param name="type">Type.</param>
        public static PropertyInfo[] GetProperties(Type type)
        {
            const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(flags);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(flags);
        }
    }
}
