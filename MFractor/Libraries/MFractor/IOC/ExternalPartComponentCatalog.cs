using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition;
using System.Linq;


namespace MFractor.IOC
{
    /// <summary>
    /// A <see cref="ComposablePartCatalog"/> implementation that allows applications built on MFractor to add external/third party interfaces into the dependency injection engine.
    /// </summary>
    class ExternalPartComponentCatalog : ComposablePartCatalog, IExternalPartRegistrar
    {
        const string creationPolicyKey = "System.ComponentModel.Composition.CreationPolicy";

        class ExternalPartDefinition<TPart> : ComposablePartDefinition
        {
            public ExternalPartDefinition(Func<TPart> partFactory, string displayName, string contractName, bool isSingleton)
            {
                if (isSingleton)
                {
                    singletonImpl = new Lazy<TPart>(partFactory);
                    PartFactory = () => singletonImpl.Value;
                }
                else
                {
                    PartFactory = partFactory;
                }

                DisplayName = displayName;
                ContractName = contractName;
                IsSingleton = isSingleton;
                metaData[creationPolicyKey] = isSingleton ? CreationPolicy.Shared : CreationPolicy.NonShared;

                exportDefinitions = new List<ExportDefinition>()
                {
                    new ExportDefinition(contractName, new Dictionary<string, object>()
                    {
                        {   "ExportTypeIdentity", typeof(TPart).FullName },
                        {  creationPolicyKey , isSingleton ? CreationPolicy.Shared : CreationPolicy.NonShared},
                    })
                };
            }

            readonly Lazy<TPart> singletonImpl;
            public Func<TPart> PartFactory { get; }
            public string DisplayName { get; }
            public string ContractName { get; }
            public bool IsSingleton { get; }

            readonly IReadOnlyList<ExportDefinition> exportDefinitions;
            public override IEnumerable<ExportDefinition> ExportDefinitions => exportDefinitions;

            public override IEnumerable<ImportDefinition> ImportDefinitions => Enumerable.Empty<ImportDefinition>();

            public override ComposablePart CreatePart()
            {
                return new ExternalPart<TPart>(PartFactory, exportDefinitions);
            }

            readonly Dictionary<string, object> metaData = new Dictionary<string, object>();
            public override IDictionary<string, object> Metadata => base.Metadata;
        }

        class ExternalPart<TPart> : ComposablePart
        {
            public ExternalPart(Func<TPart> partFactory, IReadOnlyList<ExportDefinition> exportDefinitions)
            {
                this.partFactory = partFactory;
                this.exportDefinitions = exportDefinitions;
            }

            readonly IReadOnlyList<ExportDefinition> exportDefinitions;
            readonly Func<TPart> partFactory;

            public override IEnumerable<ExportDefinition> ExportDefinitions => exportDefinitions;

            public override IEnumerable<ImportDefinition> ImportDefinitions => Enumerable.Empty<ImportDefinition>();

            public override object GetExportedValue(ExportDefinition definition)
            {
                return partFactory();
            }

            public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
            {
                // Unavailable with external parts.
            }
        }


        public void RegisterSingleton<RegisterType, RegisterImplementation>() where RegisterImplementation : RegisterType, new()
        {
            parts.Add(new ExternalPartDefinition<RegisterType>(() => new RegisterImplementation(), typeof(RegisterImplementation).FullName, typeof(RegisterType).FullName, true));
        }

        public void RegisterMultiInstance<RegisterType, RegisterImplementation>() where RegisterImplementation : RegisterType, new()
        {
            parts.Add(new ExternalPartDefinition<RegisterType>(() => new RegisterImplementation(), typeof(RegisterImplementation).FullName, typeof(RegisterType).FullName, false));
        }

        public void RegisterSingleton<RegisterType>(RegisterType instance)
        {
            parts.Add(new ExternalPartDefinition<RegisterType>(() => instance, instance.GetType().FullName, typeof(RegisterType).FullName, true));
        }

        public void RegisterMultiInstance<RegisterType>(RegisterType instance)
        {
            parts.Add(new ExternalPartDefinition<RegisterType>(() => instance, instance.GetType().FullName, typeof(RegisterType).FullName, false));
        }

        public void RegisterSingleton<RegisterType>(Func<RegisterType> factory)
        {
            parts.Add(new ExternalPartDefinition<RegisterType>(factory, typeof(RegisterType).FullName, typeof(RegisterType).FullName, true));
        }

        public void RegisterMultiInstance<RegisterType, RegisterImplementation>(Func<RegisterImplementation> factory) where RegisterImplementation : RegisterType
        {
            parts.Add(new ExternalPartDefinition<RegisterType>(() => (RegisterType)factory(), typeof(RegisterImplementation).FullName, typeof(RegisterType).FullName, true));
        }

        readonly List<ComposablePartDefinition> parts = new List<ComposablePartDefinition>();
        public override IQueryable<ComposablePartDefinition> Parts => parts.AsQueryable();
    }
}
