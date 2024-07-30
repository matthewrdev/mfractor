using System.Collections.Generic;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.Scaffolding
{
    public interface IScaffoldingContext
    {
        Solution Solution { get; }

        Project Project { get; }

        IProjectFile ProjectFile { get; }
    }
}
