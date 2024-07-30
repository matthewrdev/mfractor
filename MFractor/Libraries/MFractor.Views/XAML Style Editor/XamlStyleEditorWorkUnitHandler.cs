using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Maui.WorkUnits;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Workspace.Utilities;

namespace MFractor.Views.XamlStyleEditor
{
    class XamlStyleEditorWorkUnitHandler : WorkUnitHandler<XamlStyleEditorWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public XamlStyleEditorWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                              Lazy<IWorkEngine> workEngine,
                                              Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.workEngine = workEngine;
            this.dispatcher = dispatcher;
        }

        public async override Task<IWorkExecutionResult> OnExecute(XamlStyleEditorWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                var id = workUnit.Project.GetIdentifier();

                var dialog = new XamlStyleEditorDialog(id, workUnit.FilePath, workUnit.TargetType, workUnit.TargetTypePrefix, workUnit.Properties, workUnit.ParentStyleName, workUnit.ParentStyleType, workUnit.Platform, workUnit.TargetFiles);
                dialog.StyleName = workUnit.StyleName;
                dialog.ShowAllProperties = workUnit.ShowAllProperties;
                dialog.HelpUrl = workUnit.HelpUrl;
                dialog.ButtonLabel = workUnit.ApplyButtonLabel;
                dialog.XamlStyleEdited += (sender, e) =>
                {
                    try
                    {
                        var workUnits = workUnit.ApplyStyleDelegate(e.Platform, e.StyleName, e.TargetType, e.TargetTypePrefix, e.ParentStyleName, e.ParentStyleType, e.Properties, e.TargetFile);

                        WorkEngine.ApplyAsync(workUnits);
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                };

                dialog.Closed += (sender, e) => {
                };

                dialog.Run(RootWindowService.RootWindowFrame);
            });

            return null;
        }
    }
}
