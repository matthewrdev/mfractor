using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;

namespace MFractor.Work.WorkUnitHandlers
{
    class CopyValueToClipboardWorkUnitHandler : WorkUnitHandler<CopyValueToClipboardWorkUnit>
    {
        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IClipboard> clipboard;
        public IClipboard Clipboard => clipboard.Value;

        [ImportingConstructor]
        public CopyValueToClipboardWorkUnitHandler(Lazy<IDialogsService> dialogsService,
                                                   Lazy<IClipboard> clipboard)
        {
            this.dialogsService = dialogsService;
            this.clipboard = clipboard;
        }

        public override Task<IWorkExecutionResult> OnExecute(CopyValueToClipboardWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Clipboard.Text = workUnit.Value;

            DialogsService.StatusBarMessage(workUnit.Message);

            return Task.FromResult(default(IWorkExecutionResult));
        }
    }
}
