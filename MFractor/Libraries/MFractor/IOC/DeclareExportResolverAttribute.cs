using System;

namespace MFractor.IOC
{
    /// <summary>
    /// Declares the <see cref="IExportResolver"/> implementation to use for the app domain.
    /// <para/>
    /// When the <see cref="Resolver"/> is first used, it will locate the <see cref="DeclareExportResolverAttribute"/> in the app domain and creates the <see cref="IExportResolver"/> instance that it declares.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class DeclareExportResolverAttribute : Attribute
    {
        /// <summary>
        /// The type of 
        /// </summary>
        public Type ExportResolverType { get; }

        public DeclareExportResolverAttribute(Type exportResolverType)
        {
            ExportResolverType = exportResolverType;
        }
    }
}
