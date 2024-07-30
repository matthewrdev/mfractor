using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Work;

namespace MFractor.Code.Scaffolding
{
    public delegate void OnScaffolderConfigurationCompleted(IScaffolder scaffolder, IScaffolderConfiguration scaffolderConfiguration);

    public interface IScaffolderConfiguration
    {
        string Label { get; }

        string Description { get; }

        Type TargetScaffolderType { get; }

        Task<IReadOnlyList<IWorkUnit>> Configure(IScaffoldingContext scaffoldingContext, OnScaffolderConfigurationCompleted configurationCompleted);
    }
}