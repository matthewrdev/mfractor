using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Mvvm.BindingContextConnectors;
using MFractor.IOC;

namespace MFractor.Maui.Mvvm
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IBindingContextConnectorService))]
    class BindingContextConnectorService : PartRepository<IBindingContextConnector>, IBindingContextConnectorService
    {
        public IReadOnlyList<IBindingContextConnector> BindingContextConnectors => Parts;

        [ImportingConstructor]
        public BindingContextConnectorService(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IBindingContextConnector ResolveById(string id)
        {
            return BindingContextConnectors.FirstOrDefault(c => c.Identifier == id);
        }

        public IBindingContextConnector ResolveByName(string name)
        {
            return BindingContextConnectors.FirstOrDefault(c => c.Name == name);
        }

        public IBindingContextConnector Resolve<TBindingContextConnector>() where TBindingContextConnector : IBindingContextConnector
        {
            return BindingContextConnectors.OfType<TBindingContextConnector>().FirstOrDefault();
        }

        public IEnumerable<IBindingContextConnector> GetBindingContextConnectors(ProjectIdentifier projectIdentifier)
        {
            return BindingContextConnectors.Where(b => b.IsAvailable(projectIdentifier));
        }
    }
}