using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.Documents;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.StringsProviders
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class CSharpLocalisableStringsProvider : ILocalisableStringsProvider
    {
        public IEnumerable<ILocalisableString> RetrieveLocalisableStrings(IParsedDocument document, object semanticModel)
        {
            var csharpDocument = document as ParsedCSharpDocument;

            if (csharpDocument == null)
            {
                return Enumerable.Empty<ILocalisableString>();
            }

            var walker = new MFractor.Utilities.SyntaxWalkers.StringSyntaxWalker();

            walker.Visit(csharpDocument.GetSyntaxTree().GetRoot());

            return walker.LiteralExpressionSyntax.Select(sl => new LocalisableString(sl.Token.ValueText, csharpDocument.FilePath, sl.Span)).Where(t => t.HasValue).OrderBy(t => t.Span.Start);
        }

        public bool IsAvailable(IParsedDocument document)
        {
            return document is ParsedCSharpDocument;
        }

        public bool IsAvailable(Project project, string filePath)
        {
            if (project == null || string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return Path.GetExtension(filePath).Equals(".cs", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
