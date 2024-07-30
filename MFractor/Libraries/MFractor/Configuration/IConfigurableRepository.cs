using System;
using System.Collections.Generic;

namespace MFractor.Configuration
{
    /// <summary>
    /// A repository that provides the <see cref="IConfigurable"/>'s within the app domain.
    /// </summary>
    public interface IConfigurableRepository
    {
        IReadOnlyList<IConfigurable> Configurables { get; }
    }
}
