using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.CSharp.WorkUnits;
using MFractor.Licensing;
using MFractor.Progress;
using MFractor.Work.WorkUnits;
using MFractor.Text;
using Microsoft.CodeAnalysis.Text;
using MFractor.Work;

namespace MFractor.Views.TypeSimplificationWizard
{
    class SimplifyTypesInFileWorkUnitHandler : WorkUnitHandler<SimplifyTypesInFileWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public SimplifyTypesInFileWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                                  Lazy<IWorkEngine> workEngine,
                                                  Lazy<ITextProviderService> textProviderService,
                                                  Lazy<ILicensingService> licensingService,
                                                  Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.workEngine = workEngine;
            this.textProviderService = textProviderService;
            this.licensingService = licensingService;
            this.dispatcher = dispatcher;
        }

        public async override Task<IWorkExecutionResult> OnExecute(SimplifyTypesInFileWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                var dialog = new TypeSimplificationWizardDialog();
                dialog.SetProjectFile(workUnit.ProjectFile);

                dialog.TypeSimplicationConfirmed += (sender, e) =>
                {
                    if (!LicensingService.IsPaid)
                    {
                        WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"The Type Simplification Wizard is a Professional-only MFractor feature. Please upgrade or request a trial.", "Type Simplification Wizard"));

                        return;
                    }

                    try
                    {
                        var contentLength = TextProviderService.GetTextProvider(e.ProjectFile.FilePath).GetText().Length;

                        var replaceText = new ReplaceTextWorkUnit()
                        {
                            Span = TextSpan.FromBounds(0, contentLength),
                            Text = e.SimplifiedContent,
                            FilePath = e.ProjectFile.FilePath,
                        };

                        WorkEngine.ApplyAsync(replaceText);
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }

                    dialog.Close();
                    dialog.Dispose();
                };

                dialog.Run(RootWindowService.RootWindowFrame);
            });

            return null;
        }
    }
}
