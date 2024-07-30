using System;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.Scaffolding;
using MFractor.Commands;
using MFractor.Work;

namespace MFractor.Code
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class CodeFeatureSetDiagnostics : IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ICodeActionRepository> codeActions;
        public ICodeActionRepository CodeActions => codeActions.Value;

        readonly Lazy<ICodeAnalyserRepository> codeAnalysers;
        public ICodeAnalyserRepository CodeAnalysers => codeAnalysers.Value;

        readonly Lazy<ICommandRepository> commands;
        public ICommandRepository Commands => commands.Value;

        readonly Lazy<IScaffolderRepository> scaffolders;
        public IScaffolderRepository Scaffolders => scaffolders.Value;

        readonly Lazy<IWorkUnitHandlerRepository> workUnitHandlers;
        public IWorkUnitHandlerRepository WorkUnitHandlers => workUnitHandlers.Value;

        [ImportingConstructor]
        public CodeFeatureSetDiagnostics(Lazy<ICodeActionRepository> codeActions,
                                         Lazy<ICodeAnalyserRepository> codeAnalysers,
                                         Lazy<ICommandRepository> commands,
                                         Lazy<IWorkUnitHandlerRepository> workUnitHandlers,
                                         Lazy<IScaffolderRepository> scaffolders)
        {
            this.codeActions = codeActions;
            this.codeAnalysers = codeAnalysers;
            this.commands = commands;
            this.workUnitHandlers = workUnitHandlers;
            this.scaffolders = scaffolders;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            log?.Info("Discovered " + CodeActions.CodeActions.Count + " code actions");
            log?.Info("Discovered " + CodeAnalysers.Analysers.Count + " code analysers");
            log?.Info("Discovered " + Commands.Commands.Count + " commands");
            log?.Info("Discovered " + Scaffolders.Scaffolders.Count + " scaffolders");
            log?.Info("Discovered " + WorkUnitHandlers.WorkUnitHandlers.Count + " work unit handlers");
        }
    }
}
