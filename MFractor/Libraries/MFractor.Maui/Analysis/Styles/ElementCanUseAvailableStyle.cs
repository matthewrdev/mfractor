using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Styles;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Styles
{
    class ElementCanUseAvailableStyle : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects XAML elements that do not have a style applied and, if possible, matches them to an available style that targets the element type and also applies the same properties.";

        public override IssueClassification Classification => IssueClassification.Improvement;

        public override string Identifier => "com.mfractor.code.analysis.xaml.element_can_use_available_style";

        public override string Name => "Element Can Use Available Style";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1082";

        readonly Lazy<IApplyStyleRefactoring> applyStyleRefactoring;
        public IApplyStyleRefactoring ApplyStyleRefactoring => applyStyleRefactoring.Value;

        [ImportingConstructor]
        public ElementCanUseAvailableStyle(Lazy<IApplyStyleRefactoring> applyStyleRefactoring)
        {
            this.applyStyleRefactoring = applyStyleRefactoring;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.VisualElement.MetaType))
            {
                return default;
            }

            if (!syntax.HasAttributes)
            {
                return default;
            }

            var styleAttribute = syntax.GetAttributeByName("Style");
            if (styleAttribute != null)
            {
                return default;
            }

            if (!TryGetPreprocessor(context, out StyleAnalysisPreprocessor preprocessor))
            {
                return default;
            }

            var targetTypeStyles = preprocessor.GetStylesForTargetType(symbol, context.Document.FilePath, context.Project, context.Platform);
            if (targetTypeStyles == null || !targetTypeStyles.Any())
            {
                return default;
            }

            var applicableStyles = targetTypeStyles.Where(s => ApplyStyleRefactoring.CanApplyStyle(syntax, s)).ToList();

            if (!applicableStyles.Any())
            {
                return default;
            }

            var bundle = new ElementCanUseAvailableStyleBundle(applicableStyles);

            if (applicableStyles.Count == 1)
            {
                var message = $"This element can be simplified by applying the matching style '{applicableStyles.First().Name}'.";

                message += "\n\n" + RenderStyle(applicableStyles.First());

                return CreateIssue(message, syntax, syntax.NameSpan, bundle).AsList();
            }

            var summary = new List<string>();

            foreach (var s in applicableStyles)
            {
                summary.Add(" * " + s.Name);
            }

            return CreateIssue($"This element can be simplified by applying one of the following matching styles:\n\n" + string.Join("\n", summary), syntax, syntax.NameSpan, bundle).AsList();
        }

        string RenderStyle(IStyle style)
        {
            var description = (style.IsImplicit ? "Implict Style" : style.Name)+ " targets " + style.TargetType;

            if (style.InheritanceChain.Any())
            {
                description += " (Inherits: " + string.Join(", ", style.InheritanceChain) + ")";
            }

            description += "\n" + string.Join("\n", style.Properties.Select(p => " * " + p.Name + "='" + p.Value + "'"));

            return description;
        }
    }
}
