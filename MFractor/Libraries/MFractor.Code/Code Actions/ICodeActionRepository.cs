using System;
using System.Collections.Generic;
using MFractor.Configuration;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> implementation that provides the available <see cref="ICodeAction"/>'s in the app domain.
    /// </summary>
    public interface ICodeActionRepository : IConfigurablePartRepository<ICodeAction>
    {
        IReadOnlyList<ICodeAction> CodeActions { get; }
    }
}
