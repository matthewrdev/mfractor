using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code.WorkUnits;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Code.WorkUnitHandlers
{
    class EncapsulateXmlSyntaxWorkUnitHandler : WorkUnitHandler<EncapsulateXmlSyntaxWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<IXmlFormattingPolicyService> formattingPolicyService;
        public IXmlFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<ILineCollectionFactory> lineCollectionFactory;
        public ILineCollectionFactory LineCollectionFactory => lineCollectionFactory.Value;

        [ImportingConstructor]
        public EncapsulateXmlSyntaxWorkUnitHandler(Lazy<ITextProviderService> textProviderService,
                                                   Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                                   Lazy<IXmlFormattingPolicyService> formattingPolicyService,
                                                   Lazy<ILineCollectionFactory> lineCollectionFactory)
        {
            this.textProviderService = textProviderService;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
            this.lineCollectionFactory = lineCollectionFactory;
        }

        public override async Task<IWorkExecutionResult> OnExecute(EncapsulateXmlSyntaxWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var xmlPolicy = workUnit.FormattingPolicy ?? FormattingPolicyService.GetXmlFormattingPolicy();

            return await InsertIntoFile(workUnit.FilePath, workUnit, xmlPolicy);
        }

        Task<IWorkExecutionResult> InsertIntoFile(string filePath, EncapsulateXmlSyntaxWorkUnit workUnit, IXmlFormattingPolicy xmlPolicy)
        {
            return Task.Run<IWorkExecutionResult>(() =>
            {
                var result = new WorkExecutionResult();
                try
                {
                    var provider = TextProviderService.GetTextProvider(filePath);

                    var lines = LineCollectionFactory.Create(provider);

                    if (lines == null)
                    {
                        log?.Warning("No file content available for " + filePath + ". Unable to insert xml syntax.");
                        return default;
                    }

                    var replacements = new List<ITextReplacement>();

                    replacements = Apply(lines, workUnit, xmlPolicy);

                    replacements = replacements.OrderByDescending(c => c.Offset).ToList();

                    result.AddTextReplacements(replacements);
                    result.AddChangedFile(workUnit.FilePath);
                    result.AddAppliedWorkUnit(workUnit);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }

                return result;
            });
        }

        List<ITextReplacement> Apply(ILineCollection lines, EncapsulateXmlSyntaxWorkUnit workUnit, IXmlFormattingPolicy policy)
        {
            var changes = new List<ITextReplacement>();

            var targetSyntax = workUnit.Target;

            var targetLine = lines.GetLineAtOffset(targetSyntax.OpeningTagSpan.Start);

            var indent = LineHelper.GetIndent(targetSyntax, targetLine);

            var content = XmlSyntaxWriter.WriteNode(workUnit.NewParent, indent, policy, false, false, false);

            changes.Add(new Text.TextReplacement()
            {
                Text = content + Environment.NewLine + indent + indent,
                FilePath = workUnit.FilePath,
                Offset = targetSyntax.OpeningTagSpan.Start,
                Length = 0,
            });

            changes.Add(new Text.TextReplacement()
            {
                Text = $"{Environment.NewLine}{indent}</{workUnit.NewParent.Name.FullName}>",
                FilePath = workUnit.FilePath,
                Offset = targetSyntax.Span.End,
                Length = 0,
            });

            var targetLines = lines.GetLines(targetSyntax.Span);

            var isFirst = true;
            foreach (var line in targetLines)
            {
                if (isFirst) // Ignore first line as the parent insertion takes care of this.
                {
                    isFirst = false;
                    continue;
                }

                changes.Add(new Text.TextReplacement()
                {
                    FilePath = workUnit.FilePath,
                    Text = indent,
                    Offset = line.Span.Start,
                    Length = 0
                });
            }

            return changes;
        }
    }
}