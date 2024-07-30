using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using MFractor.Editor;
using MFractor.Editor.Utilities;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.Work;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class NavigateToFileSpanWorkUnitHandler : WorkUnitHandler<NavigateToFileSpanWorkUnit>
    {
        private DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        readonly Lazy<ITextViewService> textViewService;
        public ITextViewService TextViewService => textViewService.Value;

        readonly Lazy<IVsEditorAdaptersFactoryService> vsEditorAdaptersFactoryService;
        public IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService => vsEditorAdaptersFactoryService.Value;

        [ImportingConstructor]
        public NavigateToFileSpanWorkUnitHandler(Lazy<IDispatcher> dispatcher,
                                                Lazy<ITextViewService> textViewService,
                                                Lazy<IVsEditorAdaptersFactoryService> vsEditorAdaptersFactoryService)
        {
            this.dispatcher = dispatcher;
            this.textViewService = textViewService;
            this.vsEditorAdaptersFactoryService = vsEditorAdaptersFactoryService;
        }

        public override async Task<IWorkExecutionResult> OnExecute(NavigateToFileSpanWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                NavigateToSpan(workUnit.FilePath, workUnit.Span, workUnit.HighlightSpan);
            });

            return default;
        }


        void NavigateToSpan(string filename, Microsoft.CodeAnalysis.Text.TextSpan span, bool highlightSpan)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)DTE;
                var serviceProvider = new ServiceProvider(sp);

                IVsTextView visualStudioTextView;
                if (VsShellUtilities.IsDocumentOpen(serviceProvider, filename, Guid.Empty, out _ , out _, out var frame))
                {
                    frame.Show();
                    visualStudioTextView = VsShellUtilities.GetTextView(frame);
                }
                else
                {
                    DTE.ItemOperations.OpenFile(filename).Activate();
                    visualStudioTextView = GetIVsTextView(filename, serviceProvider);
                }

                if (visualStudioTextView != null)
                {
                    var textView = VsEditorAdaptersFactoryService.GetWpfTextView(visualStudioTextView);
                    if (textView != null)
                    {
                        if (span.Length > 0 && highlightSpan)
                        {
                            textView.SetSelection(span);
                        }
                        else
                        {
                            textView.SetCaretLocation(span.Start);
                        }
                    }
                }
                else
                {
                    log?.Warning("Failed to navigate to " + filename + ". Unable to open or activate the editor for this file.");
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }


        internal IVsTextView GetIVsTextView(string filePath, IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (VsShellUtilities.IsDocumentOpen(serviceProvider, filePath, (Guid)VSConstants.LOGVIEWID.Code_guid, out _, out _, out var windowFrame))
                {
                    return VsShellUtilities.GetTextView(windowFrame);
                }

                if (VsShellUtilities.IsDocumentOpen(serviceProvider, filePath, (Guid)VSConstants.LOGVIEWID.TextView_guid, out _, out _, out windowFrame))
                {
                    return VsShellUtilities.GetTextView(windowFrame);
                }
            }
            catch (ObjectDisposedException ex)
            {
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return null;
        }
    }
}
