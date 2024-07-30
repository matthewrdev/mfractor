using System;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;

namespace MFractor.Code.WorkUnits
{
    public class SetBuildActionWorkUnit : WorkUnit
    {
        public string BuildAction { get; set; }

        public IProjectFile ProjectFile { get; set; }
    }
}