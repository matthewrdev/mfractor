using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code.CodeGeneration;
using MFractor.Code.Formatting;
using MFractor.Code.WorkUnits;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Code.WorkUnitHandlers
{
    class InsertXmlSyntaxWorkUnitHandler : WorkUnitHandler<InsertXmlSyntaxWorkUnit>
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
        public InsertXmlSyntaxWorkUnitHandler(Lazy<ITextProviderService> textProviderService,
                                              Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                              Lazy<IXmlFormattingPolicyService> formattingPolicyService,
                                              Lazy<ILineCollectionFactory> lineCollectionFactory)
        {
            this.textProviderService = textProviderService;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
            this.lineCollectionFactory = lineCollectionFactory;
        }

        public override async Task<IWorkExecutionResult> OnExecute(InsertXmlSyntaxWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var xmlPolicy = workUnit.FormattingPolicy ?? FormattingPolicyService.GetXmlFormattingPolicy();

            return await InsertIntoFile(workUnit.FilePath, workUnit, xmlPolicy);
        }

        Task<IWorkExecutionResult> InsertIntoFile(string filePath, InsertXmlSyntaxWorkUnit workUnit, IXmlFormattingPolicy xmlPolicy)
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

                if (workUnit.Syntax is XmlAttribute)
                {
                    replacements = ApplyAttributeChange(lines, workUnit, xmlPolicy);
                }
                else
                {
                    replacements = ApplyNodeChange(lines, workUnit, xmlPolicy);
                }

                replacements = replacements.OrderByDescending(c => c.Offset).ToList();

                result.AddTextReplacements(replacements);
                result.AddChangedFile(workUnit.FilePath);
                result.AddAppliedWorkUnit(workUnit);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return Task.FromResult< IWorkExecutionResult>(result);
        }

        List<ITextReplacement> ApplyNodeChange(ILineCollection lines, InsertXmlSyntaxWorkUnit workUnit, IXmlFormattingPolicy policy)
        {
            var changes = new List<ITextReplacement>();

            var host = workUnit.HostSyntax;

            var targetLine = lines.GetLineAtOffset(host.OpeningTagSpan.Start);

            var indent = LineHelper.GetIndent(host, targetLine);

            var anchor = host.GetChildNode(c => c == workUnit.AnchorSyntax);

            var insertionNode = workUnit.Syntax as XmlNode;
            var content = XmlSyntaxWriter.WriteNode(insertionNode, indent + policy.ContentIndentString, policy, true, true, anchor == null);

            var replaceStart = host.OpeningTagSpan.End;

            var isAnchored = false;

            if (host.IsSelfClosing)
            {
                replaceStart -= 2;
                content = ">\n" + content + "\n" + indent + $"</{host.Name.FullName}>\n";
            }
            else if (anchor != null)
            {
                isAnchored = true;

                if (workUnit.InsertionLocation == InsertionLocation.End)
                {
                    replaceStart = anchor.Span.End;
                    content = "\n" + indent + policy.ContentIndentString + content;
                }
                else
                {
                    replaceStart = anchor.Span.Start;
                    content = content + "\n" + indent + policy.ContentIndentString;
                }
            }
            
            if (!isAnchored)
            {
                content = "\n" + content;
            }

            if (workUnit.InsertionLocation == InsertionLocation.End
                && !host.IsSelfClosing
                && !isAnchored)
            {
                replaceStart = host.ClosingTagSpan.Start;

                content += "\n" + indent;
            }

            changes.Add(new Text.TextReplacement()
            {
                Description = $"Inserted {insertionNode.Name.FullName} into {host.Name.FullName}",
                Text = content,
                FilePath = workUnit.FilePath,
                Offset = replaceStart,
                Length = 0,
            });

            return changes;
        }

        List<ITextReplacement> ApplyAttributeChange(ILineCollection lines, InsertXmlSyntaxWorkUnit workUnit, IXmlFormattingPolicy policy)
        {
            var placeOnNewLine = policy.AttributesInNewLine;
            var changes = new List<ITextReplacement>();

            var attribute = workUnit.Syntax as XmlAttribute;
            var host = workUnit.HostSyntax;

            var content = XmlSyntaxWriter.WriteAttribute(attribute, policy);

            var line = lines.GetLineAtOffset(host.OpeningTagSpan.Start);

            var indent = LineHelper.GetIndent(host, line);

            var offset = host.NameSpan.End;

            if (host.HasAttributes)
            {
                var anchorAttribute = host.Attributes.Last();
                if (workUnit.InsertionLocation != InsertionLocation.End)
                {
                    line = lines.GetLineAtOffset(anchorAttribute.Span.Start);

                    indent = LineHelper.GetIndent(host, line);
                    offset = anchorAttribute.Span.End;
                }
                else
                {
                    placeOnNewLine = false;
                    indent = string.Empty;
                    offset = host.NameSpan.End;
                }
            }

            if (placeOnNewLine)
            {
                content = policy.NewLineChars + indent + content;
            }
            else
            {
                content = " " + content;
            }

            changes.Add(new Text.TextReplacement()
            {
                Description = "Inserted " + attribute.ToString(),
                FilePath = workUnit.FilePath,
                Text = content,
                Offset = offset,
                Length = 0,
            });

            return changes;
        }
    }
}

