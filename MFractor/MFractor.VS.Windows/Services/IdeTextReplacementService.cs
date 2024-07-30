using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using MFractor.Progress;
using MFractor.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITextReplacementService))]
    class IdeTextReplacementService : TextReplacementService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public IdeTextReplacementService(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public override void ApplyTextReplacements(string fileName, IReadOnlyList<ITextReplacement> replacements, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               var sortedChanges = replacements.OrderByDescending((arg) => arg.Offset).ToList();

               var activeBuffer = GetActiveBufferForFile(fileName);

               if (activeBuffer != null)
               {
                   ApplyTextChangesToBuffer(sortedChanges, activeBuffer);
               }
               else
               {
                   var contentTypeFactory = Resolver.Resolve<IContentTypeRegistryService>();
                   var contentType = contentTypeFactory.GetContentType("any");
                   var textDocumentFactoryService = Resolver.Resolve<ITextDocumentFactoryService>();
                   var textDocument = textDocumentFactoryService.CreateAndLoadTextDocument(fileName, contentType);
                   var buffer = textDocument.TextBuffer;

                   if (buffer != null)
                   {
                       ApplyTextChangesToBuffer(sortedChanges, buffer);
                       textDocument.Save();
                   }
               }
           });
        }

        /// <summary>
        /// Returns an IVsTextView for the given file path, if the given file is open in Visual Studio.
        /// </summary>
        /// <param name="filePath">Full Path of the file you are looking for.</param>
        /// <returns>The IVsTextView for this file, if it is open, null otherwise.</returns>
        internal static IVsTextView GetIVsTextView(string filePath)
        {
            var dte2 = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE));
            var sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte2;
            var serviceProvider = new ServiceProvider(sp);
            if (VsShellUtilities.IsDocumentOpen(serviceProvider, filePath, Guid.Empty,
                                            out _, out _, out var windowFrame))
            {
                // Get the IVsTextView from the windowFrame.
                return VsShellUtilities.GetTextView(windowFrame);
            }

            return null;
        }
        ITextBuffer GetActiveBufferForFile(string filePath)
        {
            var textView = GetIVsTextView(filePath); //Calling the helper method to retrieve IVsTextView object.
            if (textView != null)
            {
                var editorAdapterFactoryService = Resolver.Resolve<IVsEditorAdaptersFactoryService>();

                textView.GetBuffer(out var vsTextLines); //Getting Current Text Lines 

                //Getting Buffer Adapter to get ITextBuffer which holds the current Snapshots as wel..
                return editorAdapterFactoryService.GetDocumentBuffer(vsTextLines);
            }

            return default;
        }

        void ApplyTextChangesToBuffer(List<ITextReplacement> sortedChanges, ITextBuffer buffer)
        {
            using (var edit = buffer.CreateEdit(EditOptions.None, null, null))
            {
                foreach (var change in sortedChanges)
                {
                    try
                    {
                        edit.Replace(change.Offset, change.Length, change.Text);
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }

                edit.Apply();
            }
        }
    }
}
