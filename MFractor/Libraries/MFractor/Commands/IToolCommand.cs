using System;
using System.ComponentModel.Composition;

namespace MFractor.Commands
{
    /// <summary>
    /// Implementations of <see cref="IToolCommand"/>  are automatically added to the "Tools" meny in the top menu bar.
    /// </summary>
    [InheritedExport]
    public interface IToolCommand : ICommand
    {
    }
}
