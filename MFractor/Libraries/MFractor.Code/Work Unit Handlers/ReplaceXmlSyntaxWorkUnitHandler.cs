using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Code.WorkUnits;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Code.WorkUnitHandlers
{
    class ReplaceXmlSyntaxWorkUnitHandler : WorkUnitHandler<ReplaceXmlSyntaxWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        readonly Lazy<IXmlFormattingPolicyService> formattingPolicyService;
        public IXmlFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<ILineCollectionFactory> lineCollectionFactory;
        public ILineCollectionFactory LineCollectionFactory => lineCollectionFactory.Value;

        [ImportingConstructor]
        public ReplaceXmlSyntaxWorkUnitHandler(Lazy<ITextProviderService> textProviderService,
                                               Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                               Lazy<IXmlFormattingPolicyService> formattingPolicyService,
                                               Lazy<ILineCollectionFactory> lineCollectionFactory)
        {
            this.textProviderService = textProviderService;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
            this.lineCollectionFactory = lineCollectionFactory;
        }

        public override Task<IWorkExecutionResult> OnExecute(ReplaceXmlSyntaxWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            if (workUnit.New.GetType() != workUnit.Existing.GetType())
            {
                throw new InvalidOperationException($"Cannot apply replace xml work unit: the new xml element is a {workUnit.New.GetType()} but the existing element is a {workUnit.Existing.GetType()}");
            }

            var xmlPolicy = workUnit.FormattingPolicy ?? FormattingPolicyService.GetXmlFormattingPolicy();

            var filePath = workUnit.FilePath;

            var provider = TextProviderService.GetTextProvider(filePath);

            var lines = LineCollectionFactory.Create(provider);

            if (lines == null)
            {
                log?.Warning("No file content available for " + filePath + ". Unable to insert xml syntax.");
                return default;
            }

            ITextReplacement replacement = null;
            if (workUnit.New is XmlAttribute)
            {
                var newElement = workUnit.New as XmlAttribute;
                var existingElement = workUnit.Existing as XmlAttribute;

                var newContent = XmlSyntaxWriter.WriteAttribute(newElement, xmlPolicy);

                var beginOffset = existingElement.Span.Start;
                var endOffset = existingElement.Span.End;

                replacement = new Text.TextReplacement
                {
                    FilePath = workUnit.FilePath,
                    Text = newContent,
                    Offset = beginOffset,
                    Length = endOffset - beginOffset
                };
            }
            else if (workUnit.New is XmlNode)
            {
                var newElement = workUnit.New as XmlNode;
                var existingElement = workUnit.Existing as XmlNode;

                var targetLine = lines.GetLineAtOffset(existingElement.OpeningTagSpan.Start);
                var indent = LineHelper.GetIndent(existingElement, targetLine);

                var newContent = XmlSyntaxWriter.WriteNode(newElement, indent, xmlPolicy, workUnit.ReplaceChildren, workUnit.GenerateClosingTags, false);

                replacement = new Text.TextReplacement();
                replacement.FilePath = workUnit.FilePath;
                replacement.Text = newContent;

                if (workUnit.ReplaceChildren)
                {
                    replacement.Offset = existingElement.Span.Start;
                    replacement.Length = existingElement.Span.Length;
                }
                else
                {
                    replacement.Offset = existingElement.OpeningTagSpan.Start;
                    replacement.Length = existingElement.OpeningTagSpan.Length;
                }
            }

            var result = new WorkExecutionResult();
            result.AddTextReplacement(replacement);
            result.AddAppliedWorkUnit(workUnit);
            result.AddChangedFile(workUnit.FilePath);

            return Task.FromResult<IWorkExecutionResult>(result);
        }
    }
}
