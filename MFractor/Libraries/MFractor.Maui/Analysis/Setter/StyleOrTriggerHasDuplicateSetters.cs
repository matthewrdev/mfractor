using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Setter
{
    class StyleOrTriggerHasDuplicateSetters : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.style_or_trigger_has_duplicate_setters";

        public override string Name => "Style Or Trigger Has Duplicate Setters";

        public override string Documentation => "Inspects Style and Trigger declarations and checks if there is are multiple Setter's for a property.";

        public override string DiagnosticId => "MF1060";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, 
                                                             IParsedXamlDocument document, 
                                                             IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            var isStyleOrTrigger = SymbolHelper.DerivesFrom(symbol, context.Platform.Style.MetaType)
                                    || SymbolHelper.DerivesFrom(symbol, context.Platform.TriggerBase.MetaType);
            if (!isStyleOrTrigger)
            {
                return Array.Empty<ICodeIssue>();
            }

            var setters = syntax.GetChildren(c =>
            {
                var prop = c.GetAttributeByName("Property");
                if (prop == null || !prop.HasValue)
                {
                    return false;
                }


                var type = context.XamlSemanticModel.GetSymbol(c) as ITypeSymbol;

                return SymbolHelper.DerivesFrom(type, context.Platform.Setter.MetaType);
            });

            if (!setters.Any())
            {
                return Array.Empty<ICodeIssue>();
            }

            var issues = new List<ICodeIssue>();

            foreach (var setter in setters)
            {
                var name = setter.GetAttributeByName("Property").Value.Value;
                var count = setters.Count(c => c.GetAttributeByName("Property").Value.Value == name);

                if (count > 1)
                {
                    issues.Add(CreateIssue("Multiple setters are defined for " + name + ".", setter, setter.Span));
                }
            }

            return issues;
        }
    }
}
