using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Localisation.WorkUnits;
using MFractor.Progress;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.Localisation
{
    class LocaliseDocumentWorkUnitHandler : WorkUnitHandler<LocaliseDocumentWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public LocaliseDocumentWorkUnitHandler(Lazy<IRootWindowService> rootWindowService, Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override Task<IWorkExecutionResult> OnExecute(LocaliseDocumentWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               var projects = workUnit.Projects.ToList();
               projects.Add(workUnit.Project);
               var dialog = new FileLocalisationWizardDialog(workUnit.Project, projects, workUnit.TargetSpan, workUnit.Document, workUnit.SemanticModel, workUnit.DefaultFile);

               dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
               dialog.Run(RootWindowService.RootWindowFrame);
           });

            return default;
        }
    }
}
