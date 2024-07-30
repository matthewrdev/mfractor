using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis;
using MFractor.Maui.Analysis.Styles;
using MFractor.Maui.Styles;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Styles
{
    class RemoveRedundantStylePropertyInitialisation : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(PropertyValueIsAlreadyAppliedByStyle);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.remove_redundant_style_property_initialisation";

        public override string Name => "Remove Redundant Style Property Initialisation";

        public override string Documentation => "When a property is initialised to the same value that the nodes style provides, this code fix removes the redundant assignment.";

        enum ActionId
        {
            Single,
            All,
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var suggestions = new List<ICodeActionSuggestion>()
            {
                 CreateSuggestion("Remove redundant property initialisation", ActionId.Single)
            };

            var redundantAttributes = GetAllRedundantStyleInitialisations(syntax.Parent, document, context);

            if (redundantAttributes != null && redundantAttributes.Count > 1)
            {
                suggestions.Add(CreateSuggestion("Remove all redundant property initialisations", ActionId.All));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            if (suggestion.IsAction(ActionId.Single))
            {
                return new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = new List<XmlSyntax>() { syntax }
                }.AsList();
            }
            else
            {
                var attributes = GetAllRedundantStyleInitialisations(syntax.Parent, document, context);

                return new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = attributes
                }.AsList();
            }
        }

        IReadOnlyList<XmlAttribute> GetAllRedundantStyleInitialisations(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var styleAttribute = syntax.GetAttributeByName("Style");
            if (styleAttribute == null)
            {
                return default;
            }

            var expression = context.XamlSemanticModel.GetExpression(styleAttribute) as StaticResourceExpression;
            if (expression == null || !expression.Value.HasValue)
            {
                return default;
            }

            if (!TryGetPreprocessor(context, out StyleAnalysisPreprocessor preprocessor))
            {
                return default;
            }

            var style = preprocessor.GetNamedStyle(expression.Value.Value, context.Document.FilePath, context.Project, context.Platform);
            if (style == null)
            {
                return default;
            }

            var issues = new List<ICodeIssue>();

            var redundantAttributes = new List<XmlAttribute>();

            foreach (var property in style.Properties.Properties)
            {
                var attribute = syntax.GetAttributeByName(property.Name);

                if (attribute != null && attribute.HasValue)
                {
                    var value = attribute.Value.Value;

                    var propertyValue = property.Value;

                    if (propertyValue is LiteralStylePropertyValue literalValue
                        && literalValue.Value == value)
                    {
                        redundantAttributes.Add(attribute);
                    }
                }
            }

            return redundantAttributes;
        }
    }
}