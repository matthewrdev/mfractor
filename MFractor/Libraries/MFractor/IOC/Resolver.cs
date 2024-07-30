using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Utilities;

namespace MFractor.IOC
{
    /// <summary>
    /// The <see cref="Resolver"/> is used to resolve implementations of interfaces, abstract classes and types.
    /// <para/>
    /// Under the hood, the <see cref="Resolver"/> uses Managed Extensibility Framework (MEF). See: https://docs.microsoft.com/en-us/dotnet/framework/mef/
    /// <para/>
    /// To become availabe for resolution using the <see cref="Resolver"/>, parts should be exported into MEF using the <see cref="ExportAttribute"/>.
    /// <para/>
    /// To gain access to other services, use the <see cref="ImportingConstructorAttribute"/> for constructor injection and the <see cref="ImportAttribute"/> for property injection.
    /// <para/>
    /// When using constructor injection, always prefer the use of <see cref="Lazy{T}"/> to defer the resolution of systems. This significantly improves performance by 'circuit breaking' the dependency graph resolution and deferring the final resolution of an element until it's actually used. <see cref="Lazy{T}"/> also allows the safe use of circular references between parts.
    /// <para/>
    /// If possible, please use the Resolve methods as a last resort only. They create technical debt by violating the IOC and DI principles and are difficult to unit test.
    /// <para/>
    /// For objects that cannot be exported to MEF, such as controls or IDE integration points, you can mark properties with the <see cref="ImportAttribute"/> and then call <see cref="ComposeParts(object)"/> to trigger MFractor to resolve each property.
    /// </summary>
    public static class Resolver
    {
        static readonly Logging.ILogger log = Logging.Logger.Create();

        /// <summary>
        /// The domain assembly initialiser is called prior to the resolution of the apps declared <see cref="IExportResolver"/> to allow other assemblies to "force" themselves to load.
        /// <para/>
        /// This allows the IOC engine to scan all assemblies (as they are fully loaded).
        /// <para/>
        /// The <see cref="domainAssemblyInitialiser"/> is mutable to allow overloading by each of the app heads.
        /// </summary>
        internal static Action domainAssemblyInitialiser;

        /// <summary>
        /// The underlying <see cref="IExportResolver"/> that <see cref="ExportResolver"/> uses to create the export resolver.
        /// <para/>
        /// The <see cref="exportResolver"/> is mutable to allow overloading for unit testing.
        /// </summary>
        internal static Lazy<IExportResolver> exportResolver = new Lazy<IExportResolver>(() =>
        {
            using (Profiler.Profile("Locating export resolver."))
            {
                if (domainAssemblyInitialiser != null)
                {
                    log?.Info("Loading domain assemblies for dependency resolution.");
                    try
                    {
                        domainAssemblyInitialiser();
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }

                var candiateAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                                        .Where(a => a.GetName().Name.StartsWith("mfractor", StringComparison.InvariantCultureIgnoreCase))
                                                        .Where(a =>
                                                        {
                                                            try
                                                            {
                                                                return a.GetCustomAttributes(typeof(DeclareExportResolverAttribute), true).Any();
                                                            }
                                                            catch
                                                            {
                                                                return false; // Assembly has attribute issues, skip it.
                                                            }
                                                        })
                                                        .ToList();

                if (candiateAssemblies == null || !candiateAssemblies.Any())
                {
                    var message = $"No assemblies in the AppDomain have a {nameof(DeclareExportResolverAttribute)} defined to declare the backing export resolver. MFractor cannot continue without an IExportResolver implementation.";
                    log?.Error(message);
                    throw new InvalidOperationException(message);
                }

                var assembly = candiateAssemblies.FirstOrDefault();
                if (candiateAssemblies.Count > 1)
                {
                    log?.Warning($"Multiple assemblies in the AppDomain have a {nameof(DeclareExportResolverAttribute)} defined to declare the backing export resolver. MFractor will use the first assembly.");
                    log?.Warning("The following assemblies define an export resolver: " + string.Join(", ", candiateAssemblies.Select(ca => ca.FullName)));

                    assembly = candiateAssemblies.FirstOrDefault(ca => ca.GetName().Name.StartsWith("MFractor", StringComparison.Ordinal)) ?? candiateAssemblies.FirstOrDefault();

                    if (assembly is null)
                    {
                        var message = $"After filtering the duplicate assemblies that declare an export resolver, an MFractor assembly that defines the export resolver could not be found.  MFractor cannot continue without an {nameof(IExportResolver)} implementation.";
                        log?.Error(message);
                        throw new InvalidOperationException(message);
                    }
                }

                log?.Info($"Using '{assembly.FullName}' to create MFractors {nameof(IExportResolver)}.");

                var attribute = (DeclareExportResolverAttribute)assembly.GetCustomAttributes(typeof(DeclareExportResolverAttribute), true).FirstOrDefault();

                var resolver = (IExportResolver)Activator.CreateInstance(attribute.ExportResolverType);

                resolver.Prepare();

                return resolver;
            }
        });

        /// <summary>
        /// The backing <see cref="IExportResolver"/> that provides parts for the resolver.
        /// </summary>
        /// <value>The export resolver.</value>
        public static IExportResolver ExportResolver => exportResolver.Value;

        /// <summary>
        /// Resolves an instance of the provided type.
        /// </summary>
        public static object Resolve(Type type)
        {
            if (!VerifyResolver())
            {
                return default;
            }

            return ExportResolver.GetExportedValue(type);
        }

        /// <summary>
        /// Resolve an instance of <typeparamref name="T"/>.
        /// </summary>
        public static T Resolve<T>() where T : class
        {
            if (!VerifyResolver())
            {
                return default;
            }

            return ExportResolver.GetExportedValue<T>();
        }
        /// <summary>
        /// Attempts to resolves an instance of the provided type.
        /// </summary>
        public static bool TryResolve(Type type, out object result)
        {
            result = default;

            try
            {
                result = Resolve(type);
                return result != null;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve an instance of <typeparamref name="T"/>.
        /// </summary>
        public static bool TryResolve<T>(out T result) where T : class
        {
            result = default;

            try
            {
                result = Resolve<T>();
                return result != null;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                return false;
            }
        }

        /// <summary>
        /// Resolves <typeparamref name="TType"/>, cast as <typeparamref name="TCastType"/>.
        /// </summary>
        public static TCastType Resolve<TType, TCastType>() where TType : class
                                                            where TCastType : class, TType
        {
            if (!VerifyResolver())
            {
                return default;
            }

            var result = ExportResolver.GetExportedValue<TType>();
            return result as TCastType;
        }

        /// <summary>
        /// Resolve all exported implementations of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The all.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static IEnumerable<T> ResolveAll<T>() where T : class
        {
            if (!VerifyResolver())
            {
                return Enumerable.Empty<T>();
            }

            return ExportResolver.GetExportedValues<T>();
        }

        /// <summary>
        /// Gathers the parts to be imported via the <see cref="System.ComponentModel.Composition.ImportAttribute"/> and applies them to the provided <paramref name="instance"/>.
        /// </summary>
        public static void ComposeParts(object instance)
        {
            if (!VerifyResolver())
            {
                return;
            }

            ExportResolver.ComposeParts(instance);
        }

        /// <summary>
        /// Verifies that the underlying <see cref="IExportResolver"/> is available for the <see cref="Resolver"/>.
        /// <para/>
        /// If <see cref="VerifyResolver"/> returns false, it indicates an error in the export resolver and all resolve methods will fail.
        /// </summary>
        public static bool VerifyResolver()
        {
            if (exportResolver == null)
            {
                log?.Warning("No export resolver defined");
                return false;
            }

            try
            {
                if (ExportResolver == null)
                {
                    log?.Warning("No export resolver defined");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                return false;
            }

            return true;
        }
    }
}
