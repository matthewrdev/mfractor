using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace MFractor.IOC
{
    /// <summary>
    /// An <see cref="IExportResolver"/> implementation that is composed of multiple <see cref="Assembly"/>'s.
    /// <para/>
    /// When integrating MFractor into a new product that does not use MEF, the <see cref="AssemblyCompositionExportResolver"/> can be inherited and used to provide assemblies for composition.
    /// </summary>
    public abstract class AssemblyCompositionExportResolver : BaseExportResolver
    {
        /// <summary>
        /// Prepares this <see cref="AssemblyCompositionExportResolver"/>.
        /// <para/>
        /// Internal Use Only: Calling <see cref="Prepare"/> outside of the scope of MFractors startup routine will result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        public override void Prepare()
        {
            if (CompositionRoot != null)
            {
                throw new InvalidOperationException($"The {nameof(AssemblyCompositionExportResolver)} is already prepared.");
            }

            Catalog = new AggregateCatalog();

            foreach (var assembly in Assemblies)
            {
                Catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            }

            externalParts = new ExternalPartComponentCatalog();
            RegisterExternalParts(externalParts);

            Catalog.Catalogs.Add(externalParts);

            CompositionRoot = new CompositionContainer(Catalog);
        }

        /// <summary>
        /// The assemblies that are available to MEF.
        /// <para/>
        /// These assemblies are added to the <see cref="Catalog"/> and available via the <see cref="CompositionRoot"/>.
        /// </summary>
        public abstract IEnumerable<Assembly> Assemblies { get; }

        protected IReadOnlyList<Assembly> GetExportedAppDomainAssemblies()
        {
            var availableAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var result = new List<Assembly>();

            foreach (var assembly in availableAssemblies)
            {
                try
                {
                    if (assembly.GetCustomAttributes(typeof(ExportAssemblyAttribute)).Any())
                    {
                        result.Add(assembly);
                    }
                }
                catch (Exception ex)
                {
                    // Assembly not loaded or some other issue. Ignore.
                }
            }

            return result;
        }

        protected abstract void RegisterExternalParts(IExternalPartRegistrar registrar);

        public AggregateCatalog Catalog { get; private set; }

        ExternalPartComponentCatalog externalParts;
        public IExternalPartRegistrar ExternalPartsCatalog => externalParts;

        public CompositionContainer CompositionRoot { get; private set; }

        public sealed override Lazy<T> GetExport<T>()
        {
            return CompositionRoot.GetExport<T>();
        }

        public sealed override T GetExportedValue<T>()
        {
            return CompositionRoot.GetExportedValue<T>();
        }

        public sealed override IEnumerable<T> GetExportedValues<T>()
        {
            return CompositionRoot.GetExportedValues<T>();
        }

        public sealed override Lazy<IEnumerable<T>> GetExports<T>()
        {
            return new Lazy<IEnumerable<T>>(() => CompositionRoot.GetExportedValues<T>());
        }

        public sealed override object GetExportedValue(Type type)
        {
            var provider = this;

            // get a reference to the GetExportedValue<T> method
            var methodInfo = provider.GetType()
                                     .GetMethods()
                                     .First(d => d.Name == "GetExportedValue" && d.GetParameters().Length == 0);

            // create an array of the generic types that the GetExportedValue<T> method expects
            var genericTypeArray = new Type[] { type };

            // add the generic types to the method
            methodInfo = methodInfo.MakeGenericMethod(genericTypeArray);

            // invoke GetExportedValue<type>()
            return methodInfo.Invoke(provider, null);
        }

        public sealed override IEnumerable<object> GetExportedValues(Type type)
        {
            var provider = this;

            // get a reference to the GetExportedValue<T> method
            var methodInfo = provider.GetType()
                                     .GetMethods()
                                     .First(d => d.Name == "GetExportedValues" && d.GetParameters().Length == 0);

            // create an array of the generic types that the GetExportedValues<T> method expects
            var genericTypeArray = new Type[] { type };

            // add the generic types to the method
            methodInfo = methodInfo.MakeGenericMethod(genericTypeArray);

            // invoke GetExportedValue<type>()
            return (IEnumerable<object>)methodInfo.Invoke(provider, null);
        }
    }
}
