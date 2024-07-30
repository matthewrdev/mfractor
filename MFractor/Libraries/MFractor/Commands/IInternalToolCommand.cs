using System;
using System.ComponentModel.Composition;

namespace MFractor.Commands
{
    /// <summary>
    /// Internal Use Only: An command that performs an action to assist the development of MFractor.
    /// <para/>
    /// <para/>
    /// Implementations of this command are automatically added to the Internal Tools menu.
    /// </summary>
    [InheritedExport]
    public interface IInternalToolCommand : ICommand
    {
    }
}
