using System;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace.WorkUnits
{
    public class CreateProjectFolderWorkUnit : WorkUnit
    {
        public Project Project { get; set; } 

        public string VirtualPath { get; set; }
    }
}