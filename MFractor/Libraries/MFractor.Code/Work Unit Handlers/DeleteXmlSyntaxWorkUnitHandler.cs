using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Code.WorkUnits;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Code.WorkUnitHandlers
{
    class DeleteXmlSyntaxWorkUnitHandler : WorkUnitHandler<DeleteXmlSyntaxWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ITextProviderService> textProviderService;
        ITextProviderService TextProviderService => textProviderService.Value;

        [ImportingConstructor]
        public DeleteXmlSyntaxWorkUnitHandler(Lazy<ITextProviderService> textProviderService)
        {
            this.textProviderService = textProviderService;
        }

        public override Task<IWorkExecutionResult> OnExecute(DeleteXmlSyntaxWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            if (workUnit.Syntaxes == null)
            {
                log?.Warning($"No syntax elements were provided to the {nameof(DeleteXmlSyntaxWorkUnit)}.");
                return Task.FromResult<IWorkExecutionResult>(default);
            }

            var provider = TextProviderService.GetTextProvider(workUnit.FilePath);
            if (provider == null)
            {
                log?.Warning($"Could not load the contents of {workUnit.FilePath} to delete the XML syntax elements. Does this file exist?.");
                return Task.FromResult<IWorkExecutionResult>(default);
            }

            var result = new WorkExecutionResult();

            foreach (var syntax in workUnit.Syntaxes)
            {
                if (syntax == null)
                {
                    continue;
                }

                if (TryGetSpan(syntax, provider, workUnit.RemoveUnnecessaryWhitespace, out var span))
                {
                    var replaceChange = new Text.TextReplacement
                    {
                        Length = span.Length,
                        Text = string.Empty,
                        Offset = span.Start,
                        FilePath = workUnit.FilePath,
                        MoveCaretToReplacement = false
                    };

                    result.AddTextReplacement(replaceChange);
                }
            }

            result.AddAppliedWorkUnit(workUnit);

            return Task.FromResult<IWorkExecutionResult>(result);
        }

        bool TryGetSpan(XmlSyntax syntax, ITextProvider provider, bool removeUnnecessaryWhitespace, out TextSpan span)
        {
            span = default;

            bool reduceLeadingWhitespace;

            switch (syntax)
            {
                case XmlNode node:
                    span = node.Span;
                    reduceLeadingWhitespace = true;
                    break;
                case XmlAttribute attribute:
                    span = attribute.Span;
                    reduceLeadingWhitespace = true;
                    break;
                case XmlAttributeValue value:
                    span = value.Span;
                    return true;
                default:
                    return false;
            }

            if (reduceLeadingWhitespace)
            {
                if (!removeUnnecessaryWhitespace)
                {
                    return true;
                }

                var text = provider.GetText();

                var offset = span.Start;

                if (offset >= text.Length)
                {
                    return true;
                }

                var @char = text[offset];

                if (!char.IsWhiteSpace(@char))
                {
                    offset--;
                    @char = text[offset];
                }

                while (char.IsWhiteSpace(@char))
                {
                    offset--;
                    @char = text[offset];
                }

                offset++; //  Move it off the non-whitespace character.
                span = TextSpan.FromBounds(offset, span.End);
            }

            return true;
        }
    }
}
