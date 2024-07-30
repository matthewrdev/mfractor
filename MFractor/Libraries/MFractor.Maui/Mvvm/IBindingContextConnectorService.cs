using System;
using System.Collections.Generic;
using MFractor.Maui.Mvvm.BindingContextConnectors;

namespace MFractor.Maui.Mvvm
{
    public interface IBindingContextConnectorService
    {
        IReadOnlyList<IBindingContextConnector> BindingContextConnectors { get; }

        IBindingContextConnector ResolveById(string id);

        IBindingContextConnector ResolveByName(string name);

        IBindingContextConnector Resolve<TBindingContextConnector>() where TBindingContextConnector : IBindingContextConnector;

        IEnumerable<IBindingContextConnector> GetBindingContextConnectors(ProjectIdentifier projectIdentifier);
    }
}
