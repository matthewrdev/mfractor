using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Progress;
using MFractor.Text;
using MFractor.VS.Mac.Progress;
using Microsoft.VisualStudio.Text;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Refactoring;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITextReplacementService))]
    class IdeTextReplacementService : TextReplacementService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public override void ApplyTextReplacements(string fileName,
                                                   IReadOnlyList<ITextReplacement> replacements,
                                                   IProgressMonitor progressMonitor)
        {
            var sortedChanges = replacements.OrderByDescending((arg) => arg.Offset).ToList();

            var openDoc = IdeApp.Workbench.Documents.FirstOrDefault((document) => document.FileName == fileName);

            if (openDoc != null)
            {
                var buffer = openDoc.GetContent<ITextBuffer>();
                if (buffer != null)
                {
                    ApplyTextChangesToBuffer(sortedChanges, buffer);
                    return;
                }
            }

            if (!File.Exists(fileName))
            {
                log?.Warning($"Unable to apply the provided text changes to '{fileName}' as it does not exist.");
                log?.Warning($"{replacements.Count} text replacement changes will be discarded.");
                return;
            }

            var content = File.ReadAllText(fileName);

            foreach (var change in sortedChanges)
            {
                if (change.Length > 0)
                {
                    content = content.Remove(change.Offset, change.Length);
                }

                content = content.Insert(change.Offset, change.Text);
            }

            File.WriteAllText(fileName, content);
        }

        void ApplyTextChangesToBuffer(List<ITextReplacement> replacements, ITextBuffer buffer)
        {
            AppKit.NSApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                using (var edit = buffer.CreateEdit(EditOptions.None, null, null))
                {
                    foreach (var replacement in replacements)
                    {
                        try
                        {
                            edit.Replace(replacement.Offset, replacement.Length, replacement.Text);
                        }
                        catch (Exception ex)
                        {
                            log?.Exception(ex);
                        }
                    }
                    edit.Apply();
                }
            });
        }
    }
}
