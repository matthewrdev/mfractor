using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis
{
    class UnknownFieldModifer : XamlCodeAnalyser
    {
        readonly IReadOnlyDictionary<string, string> fieldModifiers = new Dictionary<string, string>()
        {
           { "private", "Specifies that the generated field for the XAML element is accessible only within the body of the class in which it is declared." },
           { "public", "Specifies that the generated field for the XAML element has no access restrictions." },
           { "protected", "Specifies that the generated field for the XAML element is accessible within its class and by derived class instances." },
           { "internal", "Specifies that the generated field for the XAML element is accessible only within types in the same assembly." },
           { "notpublic", "Specifies that the generated field for the XAML element is accessible only within types in the same assembly." },
        };

        public override string Documentation
        {
            get
            {
                var message = "Inspects occurances of the `x:FieldModifer` attribute and validates that the value is one of the following keywords.<\br>";

                message += "<ul>";
                foreach (var pair in fieldModifiers)
                {
                    message +=  "<li>" + pair.Key + ": " + pair.Value + "</li>";
                }
                message += "</ul>";

                return message;
            }
        }

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.unknown_field_modifier";

        public override string Name => "Unknown Field Modifier";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1085";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!syntax.HasValue)
            {
                return null;
            }

            if (syntax.Name.FullName != "x:FieldModifier")
            {
                return null;
            }

            if (fieldModifiers.ContainsKey(syntax.Value.Value))
            {
                return null;
            }

            var message = string.Empty;

            var match = SuggestionHelper.FindBestSuggestion(syntax.Value.Value, fieldModifiers.Keys);

            if (!string.IsNullOrEmpty(match))
            {
                message = $"\n\nDid you mean {match}?";
            }

            return CreateIssue($"{syntax.Value} is not a valid field modifier." + message, syntax, syntax.Value.Span).AsList();
        }
    }
}
